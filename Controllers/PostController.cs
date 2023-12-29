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
using Blog.Services.Firebase.Storage;
using Google.Cloud.Storage.V1;
using System.Text;
using Google.Apis.Storage.v1.Data;

namespace Blog.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly StorageFile _storageFile;

        public PostController(BlogDbContext context, UserManager<IdentityUser> userManager, StorageFile storageFile)
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
               .Include(p => p.ImageLinks)
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
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostClassifies)
                    .ThenInclude(ps => ps.Tag)
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
            var userId = _userManager.GetUserId(User);
            ViewData["UserId"] = userId;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorId, Title, Content")] Post post, IFormFile imageFile)
        // chú ý imageFile trùng tên với name của input
        {
            if (ModelState.IsValid)
            {
                var postId = new Guid(); // Sử dụng để tạo post và Fk cho table Images
                // Thêm các giá trị cho post
                post.Id = postId;
                post.Slug = imageFile.FileName.ToLower();
                post.CreateAt = DateTime.Now;
                post.UpdateAt = DateTime.Now;
                await _context.AddAsync(post);

                // Xử lí ảnh
                if (imageFile != null)
                {
                    // Upload ảnh lên firebase
                    var stream = imageFile.OpenReadStream();
                    var guid = Guid.NewGuid();
                    var fileName = $"{imageFile.FileName}_{guid}";
                    var uri = await _storageFile.UploadImage(fileName, stream);

                    // Tạo record ImageLink
                    var imageLink = new ImageLink
                    {
                        Id = guid,
                        Name = imageFile.FileName,
                        PostId = post.Id,
                        Link = uri
                    };
                    await _context.AddAsync(imageLink);
                }
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
