@echo off
REM =====================================================================
REM  ARS - Khoi dong app + SSL tu dong (Let's Encrypt)
REM  PHAI chay bang quyen Administrator (chuot phai > Run as administrator)
REM =====================================================================
cd /d "%~dp0"

echo [1/3] Giai phong cong 80/443 (tat IIS neu dang chay)...
net stop W3SVC >nul 2>&1

echo [2/3] Khoi dong LocalDB...
"C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe" start MSSQLLocalDB

REM --- Ket noi database (LocalDB, chay duoi Administrator) ---
set ConnectionStrings__MyCnn=Server=(localdb)\MSSQLLocalDB;Database=ARSDB;Trusted_Connection=True;TrustServerCertificate=True

REM --- SSL tu dong Let's Encrypt (sua email neu muon) ---
set LettuceEncrypt__AcceptTermsOfService=true
set LettuceEncrypt__EmailAddress=admin@arsrecruit.com
set LettuceEncrypt__DomainNames__0=arsrecruit.com
set LettuceEncrypt__DomainNames__1=www.arsrecruit.com

REM --- Dang nhap Google (OAuth) ---
set GoogleAuth__ClientId=113080090949-7o8sv0me6q5aoco3g2uvpqf4q5e8b4g3.apps.googleusercontent.com
set GoogleAuth__ClientSecret=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

REM --- Gui OTP qua email (PrivateEmail) ---
set EmailOtp__SmtpHost=mail.privateemail.com
set EmailOtp__Port=465
set EmailOtp__Username=noreply@arsrecruit.com
set EmailOtp__Password=@Quy12345
set EmailOtp__FromEmail=noreply@arsrecruit.com
set EmailOtp__FromName=ARS Recruitment

REM --- Cong thanh toan VNPAY (Sandbox) ---
set VnPay__TmnCode=UBLAMUHP
set VnPay__HashSecret=ILSIPGOOTOMMXYTVEJHXPNTHYEBBPXGK
set VnPay__ReturnUrl=https://arsrecruit.com/VnPay/Return

echo [3/3] Chay ARS tren cong 80 + 443 ...
echo.
dotnet WebApp.dll

pause
