using System.Net;
using System.Net.Mail;

namespace LexiConnect.Models
{
    public class EmailSender : ISender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var server = _config["Email:SmtpServer"];
            var port = _config["Email:Port"];
            var from = _config["Email:From"];

            var smtpClient = new SmtpClient(server)
            {
                Port = int.Parse(_config["Email:Port"]),
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"]
                ),
                EnableSsl = true,
                UseDefaultCredentials = false,
                Timeout = 10000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
            smtpClient.Dispose();
        }

        // Enhanced method to send welcome email with beautiful HTML template
        public async Task SendWelcomeEmailAsync(string toEmail, string userName, string confirmationUrl)
        {
            var subject = "Welcome to LexiConnect - Confirm Your Email";
            var htmlBody = CreateWelcomeEmailTemplate(userName, confirmationUrl);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        // Method to send password reset email
        public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetUrl)
        {
            var subject = "Reset Your LexiConnect Password";
            var htmlBody = CreatePasswordResetEmailTemplate(userName, resetUrl);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        private static string CreateWelcomeEmailTemplate(string userName, string confirmationUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to LexiConnect</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
            overflow: hidden;
            margin-top: 20px;
            margin-bottom: 20px;
        }}
        
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 20px;
            text-align: center;
            color: white;
        }}
        
        .logo {{
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
        }}
        
        .header-subtitle {{
            font-size: 16px;
            opacity: 0.9;
            margin: 0;
        }}
        
        .content {{
            padding: 40px 30px;
            line-height: 1.6;
            color: #333333;
        }}
        
        .greeting {{
            font-size: 24px;
            color: #667eea;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        
        .message {{
            font-size: 16px;
            margin-bottom: 30px;
            color: #555555;
        }}
        
        .cta-button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: bold;
            font-size: 16px;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            text-align: center;
            margin: 20px 0;
        }}
        
        .cta-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
        }}
        
        .features {{
            background-color: #f8f9ff;
            padding: 30px;
            margin: 30px 0;
            border-radius: 8px;
            border-left: 4px solid #667eea;
        }}
        
        .feature-list {{
            list-style: none;
            padding: 0;
            margin: 0;
        }}
        
        .feature-item {{
            padding: 8px 0;
            font-size: 15px;
            color: #555555;
        }}
        
        .feature-item::before {{
            content: '✓';
            color: #667eea;
            font-weight: bold;
            margin-right: 10px;
        }}
        
        .footer {{
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }}
        
        .footer-text {{
            font-size: 14px;
            color: #6c757d;
            margin: 5px 0;
        }}
        
        .social-links {{
            margin: 20px 0;
        }}
        
        .social-link {{
            display: inline-block;
            margin: 0 10px;
            color: #667eea;
            text-decoration: none;
            font-weight: bold;
        }}
        
        .divider {{
            height: 2px;
            background: linear-gradient(to right, transparent, #667eea, transparent);
            margin: 30px 0;
            border: none;
        }}
        
        @media only screen and (max-width: 600px) {{
            .email-container {{
                margin: 10px;
                border-radius: 8px;
            }}
            
            .content {{
                padding: 30px 20px;
            }}
            
            .header {{
                padding: 30px 20px;
            }}
            
            .logo {{
                font-size: 28px;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='logo'>LexiConnect</div>
            <p class='header-subtitle'>Your Gateway to Seamless Communication</p>
        </div>
        
        <div class='content'>
            <h2 class='greeting'>Welcome, {userName}! 🎉</h2>
            
            <p class='message'>
                Thank you for joining LexiConnect! We're thrilled to have you as part of our growing community. 
                You're just one step away from unlocking all the amazing features we have to offer.
            </p>
            
            <div style='text-align: center;'>
                <a href='{confirmationUrl}' class='cta-button'>
                    Confirm Your Email Address
                </a>
            </div>
            
            <hr class='divider'>
            
            <div class='features'>
                <h3 style='color: #667eea; margin-top: 0;'>What's waiting for you:</h3>
                <ul class='feature-list'>
                    <li class='feature-item'>Connect with friends and colleagues instantly</li>
                    <li class='feature-item'>Share ideas and collaborate seamlessly</li>
                    <li class='feature-item'>Access advanced communication tools</li>
                    <li class='feature-item'>Join communities that match your interests</li>
                    <li class='feature-item'>Secure and private messaging</li>
                </ul>
            </div>
            
            <p class='message'>
                If you didn't create this account, please ignore this email. The account will not be activated 
                without email confirmation.
            </p>
            
            <p style='font-size: 14px; color: #888;'>
                <strong>Having trouble with the button?</strong> Copy and paste this link into your browser:<br>
                <span style='word-break: break-all; color: #667eea;'>{confirmationUrl}</span>
            </p>
        </div>
        
        <div class='footer'>
            <div class='social-links'>
                <a href='#' class='social-link'>Facebook</a>
                <a href='#' class='social-link'>Twitter</a>
                <a href='#' class='social-link'>LinkedIn</a>
            </div>
            <p class='footer-text'>© 2025 LexiConnect. All rights reserved.</p>
            <p class='footer-text'>You received this email because you signed up for LexiConnect.</p>
            <p class='footer-text'>
                <a href='#' style='color: #667eea; text-decoration: none;'>Unsubscribe</a> | 
                <a href='#' style='color: #667eea; text-decoration: none;'>Privacy Policy</a>
            </p>
        </div>
    </div>
</body>
</html>";
        }

        private static string CreatePasswordResetEmailTemplate(string userName, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password - LexiConnect</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
            min-height: 100vh;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
            overflow: hidden;
            margin-top: 20px;
            margin-bottom: 20px;
        }}
        
        .header {{
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
            padding: 40px 20px;
            text-align: center;
            color: white;
        }}
        
        .logo {{
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
        }}
        
        .content {{
            padding: 40px 30px;
            line-height: 1.6;
            color: #333333;
        }}
        
        .greeting {{
            font-size: 24px;
            color: #ff6b6b;
            margin-bottom: 20px;
            font-weight: 600;
        }}
        
        .message {{
            font-size: 16px;
            margin-bottom: 30px;
            color: #555555;
        }}
        
        .cta-button {{
            display: inline-block;
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: bold;
            font-size: 16px;
            box-shadow: 0 4px 15px rgba(255, 107, 107, 0.4);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            text-align: center;
            margin: 20px 0;
        }}
        
        .warning-box {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 8px;
            padding: 20px;
            margin: 30px 0;
            color: #856404;
        }}
        
        .footer {{
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }}
        
        .footer-text {{
            font-size: 14px;
            color: #6c757d;
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='logo'>LexiConnect</div>
            <p style='margin: 0; opacity: 0.9;'>Password Reset Request</p>
        </div>
        
        <div class='content'>
            <h2 class='greeting'>Hello, {userName}</h2>
            
            <p class='message'>
                We received a request to reset your password for your LexiConnect account. 
                If you made this request, click the button below to reset your password:
            </p>
            
            <div style='text-align: center;'>
                <a href='{resetUrl}' class='cta-button'>
                    Reset Your Password
                </a>
            </div>
            
            <div class='warning-box'>
                <strong>⚠️ Security Notice:</strong><br>
                This password reset link will expire in 24 hours for your security. 
                If you didn't request this reset, please ignore this email and your password will remain unchanged.
            </div>
            
            <p style='font-size: 14px; color: #888;'>
                <strong>Having trouble with the button?</strong> Copy and paste this link into your browser:<br>
                <span style='word-break: break-all; color: #ff6b6b;'>{resetUrl}</span>
            </p>
        </div>
        
        <div class='footer'>
            <p class='footer-text'>© 2025 LexiConnect. All rights reserved.</p>
            <p class='footer-text'>This is an automated security email.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}