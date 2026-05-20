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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();

            // Проверяем авторизацию
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            LoadUserInfo();
            LoadUserReviews();
        }

        private void LoadUserInfo()
        {
            try
            {
                var user = Core.CurrentUser;

                TxtLogin.Text = user.Login;
                TxtDisplayName.Text = user.DisplayName;
                TxtEmail.Text = user.Email;
                TxtCreatedAt.Text = user.CreatedAt.ToString("dd.MM.yyyy");

                // Определяем роль
                string roleName = "";
                switch (user.RoleID)
                {
                    case 1:
                        roleName = "Администратор";
                        break;
                    case 2:
                        roleName = "Пользователь";
                        // Показываем кнопку подачи заявки
                        BtnRequestAuthor.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        roleName = "Автор";
                        break;
                    default:
                        roleName = "Неизвестно";
                        break;
                }
                TxtRole.Text = roleName;

                // Проверяем статус заморозки
                if (user.IsFrozen == true)
                {
                    TxtStatus.Text = "❌ Аккаунт заморожен";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Red;

                    // Показываем блок с предупреждением
                    BorderFreezeWarning.Visibility = Visibility.Visible;

                    // Ищем причину заморозки (можно добавить поле в таблицу Users)
                    TxtFreezeReason.Text = "Причина заморозки: нарушение правил платформы.\n" +
                                           "Вы можете оспорить заморозку, отправив заявку администратору.";
                }
                else
                {
                    TxtStatus.Text = "✅ Аккаунт активен";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserReviews()
        {
            try
            {
                var reviews = Core.Context.Reviews
                    .Where(r => r.UserID == Core.CurrentUser.UserID)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                if (reviews.Any())
                {
                    ReviewsItemsControl.Visibility = Visibility.Visible;
                    TxtNoReviews.Visibility = Visibility.Collapsed;
                    ReviewsItemsControl.ItemsSource = reviews;
                }
                else
                {
                    ReviewsItemsControl.Visibility = Visibility.Collapsed;
                    TxtNoReviews.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, не подавал ли пользователь уже заявку
            var existingRequest = Core.Context.AuthorRoleRequests
                .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID && r.IsProcessed == false);

            if (existingRequest != null)
            {
                MessageBox.Show("Вы уже подавали заявку на роль автора. Ожидайте рассмотрения.",
                               "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите подать заявку на роль автора?\n" +
                                        "После получения роли автора вы сможете публиковать свои книги.",
                                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AuthorRoleRequests request = new AuthorRoleRequests
                    {
                        UserID = Core.CurrentUser.UserID,
                        RequestDate = DateTime.Now,
                        IsProcessed = false,
                        IsApproved = null
                    };

                    Core.Context.AuthorRoleRequests.Add(request);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Заявка отправлена администратору. Ожидайте рассмотрения.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    BtnRequestAuthor.IsEnabled = false;
                    BtnRequestAuthor.Content = "Заявка отправлена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отправки заявки: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, не подавал ли пользователь уже заявку на разморозку
            var existingRequest = Core.Context.UnfreezeRequests
                .FirstOrDefault(r => r.UserID == Core.CurrentUser.UserID &&
                                    r.IsProcessed == false &&
                                    r.BookID == null);

            if (existingRequest != null)
            {
                MessageBox.Show("Вы уже подавали заявку на снятие заморозки. Ожидайте рассмотрения.",
                               "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Создаем окно для ввода причины
            var reasonWindow = new ReasonInputWindow("разморозку аккаунта");
            reasonWindow.Title = "Оспаривание заморозки";
            reasonWindow.Owner = Application.Current.MainWindow;

            if (reasonWindow.ShowDialog() == true)
            {
                try
                {
                    UnfreezeRequests request = new UnfreezeRequests
                    {
                        UserID = Core.CurrentUser.UserID,
                        BookID = null, // null означает заявка на разморозку аккаунта
                        Reason = reasonWindow.Reason,
                        RequestDate = DateTime.Now,
                        IsProcessed = false,
                        IsApproved = null
                    };

                    Core.Context.UnfreezeRequests.Add(request);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Заявка на снятие заморозки отправлена администратору.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    BtnAppealFreeze.IsEnabled = false;
                    BtnAppealFreeze.Content = "Заявка отправлена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отправки заявки: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnGoToBook_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int bookId = (int)button.Tag;

            NavigationService?.Navigate(new BookPage(bookId));
        }
    }
}
