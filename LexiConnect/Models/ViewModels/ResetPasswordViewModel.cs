namespace LexiConnect.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string UserId { get; set; }
        public string Code { get; set; }
    }
}
