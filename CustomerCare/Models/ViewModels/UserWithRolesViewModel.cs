namespace CustomerCare.Models.ViewModels
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
