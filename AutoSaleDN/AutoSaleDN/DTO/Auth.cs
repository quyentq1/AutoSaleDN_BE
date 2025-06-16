namespace AutoSaleDN.DTO
{
    public class Auth
    {
        public class RegisterDto
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; } = "Customer";
            public string? Mobile { get; set; }
        }

        public class LoginDto
        {
            public string Name { get; set; }
            public string Password { get; set; }
        }

        public class ForgotPasswordDto
        {
            public string Email { get; set; }
        }

        public class VerifyOtpDto
        {
            public string Email { get; set; }
            public string Otp { get; set; }
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string Otp { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
