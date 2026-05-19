using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UP17Anisimov.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string email = txtEmail.Text.Trim();
            string displayName = txtDisplayName.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Проверки
            if (string.IsNullOrEmpty(login))
            {
                ShowError("Введите логин");
                return;
            }

            if (string.IsNullOrEmpty(email) | !email.Contains("@") | !email.Contains(".") | email.IndexOf('@') <= 0 | email.IndexOf('@')>email.IndexOf('.'))
            {
                ShowError("Введите email правильно!");
                return;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                ShowError("Введите отображаемое имя");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            // Проверка уникальности логина
            if (Core.Context.Users.Any(u => u.Login == login))
            {
                ShowError("Пользователь с таким логином уже существует");
                return;
            }

            // Проверка уникальности email
            if (Core.Context.Users.Any(u => u.Email == email))
            {
                ShowError("Пользователь с таким email уже существует");
                return;
            }

            // Создание нового пользователя
            var newUser = new Users
            {
                Login = login,
                Email = email,
                DisplayName = displayName,
                PasswordHash = password, // В реальном проекте нужно хэшировать!
                RoleID = 1, // Обычная роль
                CreatedAt = DateTime.Now
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            // Автоматический вход
            Core.CurrentUser = newUser;

            NavigationService.Navigate(new CatalogPage());
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}
