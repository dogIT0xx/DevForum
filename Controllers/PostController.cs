using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Blog.Utils;
using Microsoft.AspNetCore.Identity;
using Google.Cloud.Storage.V1;
using System.Text;
using Google.Apis.Storage.v1.Data;
using Blog.Entities;
using Blog.Services.Firebase;
using Blog.Models.Post;
using Microsoft.Extensions.Hosting;

namespace Blog.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IStorageFile _storageFile;

        public PostController(BlogDbContext context, UserManager<IdentityUser> userManager, IStorageFile storageFile)
        {
            _context = context;
            _userManager = userManager;
            _storageFile = storageFile;
        }

        // GET: Post
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, int? pageNumber)
        {
            var posts = _context.Posts
               .Include(p => p.Author)
               .Include(p => p.PostClassifies)
                   .ThenInclude(ps => ps.Tag)
               .Include(pi => pi.PostImages)
               .AsNoTracking();

            //  Xử lí search
            if (!searchString.IsNullOrEmpty())
            {
                // Tìm kiếm các bài viết có tilte, và Author chứa searchString
                posts = posts
                    .Where(p =>
                        p.Title.Contains(searchString!) ||
                        p.Author.UserName!.Contains(searchString!));
            }

            // Xử lí phân trang
            var pageSize = 12;
            var paginatedPost = await PaginatedList<Post>.CreateSync(posts, pageNumber ?? 1, pageSize);

            ViewData["CurrentFilter"] = searchString; // Giữ current fillter của  search string trước đó
            return View(paginatedPost);
        }

        // GET: Post/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostClassifies)
                    .ThenInclude(ps => ps.Tag)
                .Include(p => p.PostImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostModel inputModel)
        // chú ý imageFile trùng tên với name của input
        {
            if (ModelState.IsValid)
            {
                // Get info current user
                var user = await _userManager.GetUserAsync(User);
                var newPost = new Post()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = user!.Id,
                    Title = inputModel.Title,
                    Slug = inputModel.Title.ToLower().Replace(" ", "-"),
                    Content = inputModel.Content,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now
                };

                // Xử lí ảnh
                var thumbnailFile = inputModel.Thumbnail;
                if (thumbnailFile != null)
                {
                    // Upload ảnh lên firebase
                    var stream = thumbnailFile.OpenReadStream();
                    var guid = Guid.NewGuid();

                    // Xử lí lưu tên ảnh
                    var extension = Path.GetExtension(thumbnailFile.FileName);
                    var fileName = $"{Path.GetFileNameWithoutExtension(thumbnailFile.FileName)}_{guid}{extension}";
                    await _storageFile.UploadImageAsync(fileName, stream);

                    //// Tạo record PostImage
                    //var newPostImage = new PostImage
                    //{
                    //    Id = guid,
                    //    PostId = newPost.Id,
                    //    Path = $"/{fileName}",
                    //};
                    //await _context.AddAsync(newPostImage);

                    newPost.ThumbnailPath = $"/{fileName}";
                }
                await _context.AddAsync(newPost);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,AuthorId,CreateAt,UpdateAt,Slug,Title,Content")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(Guid id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
