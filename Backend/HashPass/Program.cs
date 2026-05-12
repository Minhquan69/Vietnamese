var pwd = args.Length > 0 ? args[0] : "DemoViet2026!";
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(pwd));

