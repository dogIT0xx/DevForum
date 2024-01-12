namespace Blog.Utils
{
    public static class RegexPatterns
    {
        public const string Email = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"; 
        public const string PhoneNumber = @"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$";
    }
}
