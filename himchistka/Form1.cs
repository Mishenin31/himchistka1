using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using himchistka.Models;
using himchistka.Services;

namespace himchistka
{
    public partial class Form1 : Form
    {
        private readonly DatabaseService _databaseService = new DatabaseService();
        private readonly AuthService _authService;
        private readonly CatalogService _catalogService;

        private User _currentUser;
        private readonly BindingSource _catalogBinding = new BindingSource();
        private readonly BindingSource _cartBinding = new BindingSource();
        private readonly BindingSource _ordersBinding = new BindingSource();
        private readonly BindingSource _usersBinding = new BindingSource();
        private readonly List<OrderItem> _cartItems = new List<OrderItem>();

        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblStatus;

        private TextBox txtNameProfile;
        private TextBox txtEmailProfile;
        private TextBox txtPhoneProfile;
        private TextBox txtPasswordProfile;
        private Button btnSaveProfile;

        private TextBox txtSearch;
        private ComboBox cmbSort;
        private DataGridView dgvCatalog;
        private Button btnAddToCart;

        private DataGridView dgvCart;
        private Button btnRemoveFromCart;
        private Button btnCheckout;
        private Panel pnlBooking;
        private MonthCalendar calBookingDate;
        private ComboBox cmbBookingHour;
        private ComboBox cmbDurationHours;
        private CheckBox chkQueueBooking;
        private Label lblQueuePrice;
        private Label lblQueueHint;

        private DataGridView dgvOrders;
        private DataGridView dgvUsers;
        private Button btnPromoteToManager;
        private Button btnPromoteToAdmin;

        private Button btnAddProduct;
        private Button btnEditProduct;
        private Button btnDeleteProduct;

        private TabControl tabMain;
        private TabPage tabProfile;
        private TabPage tabCatalog;
        private TabPage tabCart;
        private TabPage tabOrders;
        private TabPage tabUsers;

        public Form1()
        {
            _authService = new AuthService(_databaseService);
            _catalogService = new CatalogService(_databaseService);

            InitializeComponent();
            BuildUi();
            RefreshAll();
            ApplyRoleVisibility();
        }

        private void BuildUi()
        {
            Text = "Химчистка Pro";
            Width = 1200;
            Height = 760;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);

            var loginPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10), BackColor = Color.FromArgb(242, 248, 255) };
            Controls.Add(loginPanel);

            loginPanel.Controls.Add(new Label { Text = "Логин (email/телефон)", Left = 10, Top = 8, Width = 170 });
            txtLogin = new TextBox { Left = 10, Top = 30, Width = 180, Name = "txtLogin" };
            loginPanel.Controls.Add(txtLogin);

            loginPanel.Controls.Add(new Label { Text = "Пароль", Left = 205, Top = 8, Width = 80 });
            txtPassword = new TextBox { Left = 205, Top = 30, Width = 140, Name = "txtPassword", UseSystemPasswordChar = true };
            loginPanel.Controls.Add(txtPassword);

            btnLogin = new Button { Left = 360, Top = 27, Width = 90, Text = "Войти", Name = "btnLogin" };
            btnLogin.Click += (_, __) => ExecuteSafe(Login);
            loginPanel.Controls.Add(btnLogin);

            btnRegister = new Button { Left = 460, Top = 27, Width = 130, Text = "Регистрация", Name = "btnRegister" };
            btnRegister.Click += (_, __) => ExecuteSafe(Register);
            loginPanel.Controls.Add(btnRegister);

            lblStatus = new Label { Left = 610, Top = 32, Width = 560, Font = new Font(Font.FontFamily, 10f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 90, 170) };
            loginPanel.Controls.Add(lblStatus);

            tabMain = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            Controls.Add(tabMain);

            tabProfile = new TabPage("Личный кабинет");
            tabCatalog = new TabPage("Каталог");
            tabCart = new TabPage("Корзина");
            tabOrders = new TabPage("Заказы");
            tabUsers = new TabPage("Пользователи");

            tabMain.TabPages.Add(tabProfile);
            tabMain.TabPages.Add(tabCatalog);
            tabMain.TabPages.Add(tabCart);
            tabMain.TabPages.Add(tabOrders);
            tabMain.TabPages.Add(tabUsers);

            BuildProfileTab();
            BuildCatalogTab();
            BuildCartTab();
            BuildOrdersTab();
            BuildUsersTab();
        }

        private void BuildProfileTab()
        {
            tabProfile.Controls.Add(new Label { Text = "Имя", Left = 25, Top = 30, Width = 120 });
            txtNameProfile = new TextBox { Left = 180, Top = 26, Width = 350, Name = "txtNameProfile" };
            tabProfile.Controls.Add(txtNameProfile);

            tabProfile.Controls.Add(new Label { Text = "Email", Left = 25, Top = 80, Width = 120 });
            txtEmailProfile = new TextBox { Left = 180, Top = 76, Width = 350, Name = "txtEmailProfile" };
            tabProfile.Controls.Add(txtEmailProfile);

            tabProfile.Controls.Add(new Label { Text = "Телефон", Left = 25, Top = 130, Width = 120 });
            txtPhoneProfile = new TextBox { Left = 180, Top = 126, Width = 350, Name = "txtPhoneProfile" };
            tabProfile.Controls.Add(txtPhoneProfile);

            tabProfile.Controls.Add(new Label { Text = "Пароль", Left = 25, Top = 180, Width = 120 });
            txtPasswordProfile = new TextBox { Left = 180, Top = 176, Width = 350, Name = "txtPasswordProfile" };
            tabProfile.Controls.Add(txtPasswordProfile);

            btnSaveProfile = new Button { Left = 180, Top = 230, Width = 200, Height = 35, Text = "Сохранить изменения", Name = "btnSaveChanges" };
            btnSaveProfile.Click += (_, __) => ExecuteSafe(SaveProfile);
            tabProfile.Controls.Add(btnSaveProfile);
        }

        private void BuildCatalogTab()
        {
            txtSearch = new TextBox { Left = 20, Top = 20, Width = 250, Name = "txtSearch" };
            txtSearch.TextChanged += (_, __) => RefreshCatalog();
            tabCatalog.Controls.Add(txtSearch);

            cmbSort = new ComboBox { Left = 280, Top = 20, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSort.Items.AddRange(new object[] { "Без сортировки", "Название", "Цена по возрастанию", "Цена по убыванию" });
            cmbSort.SelectedIndex = 0;
            cmbSort.SelectedIndexChanged += (_, __) => RefreshCatalog();
            tabCatalog.Controls.Add(cmbSort);

            dgvCatalog = new DataGridView
            {
                Left = 20,
                Top = 60,
                Width = 1120,
                Height = 500,
                AutoGenerateColumns = true,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvCatalog.DataBindingComplete += (_, __) => HighlightPremiumProducts();
            tabCatalog.Controls.Add(dgvCatalog);

            btnAddToCart = new Button { Left = 20, Top = 575, Width = 180, Height = 38, Text = "Добавить в корзину" };
            btnAddToCart.Click += (_, __) => ExecuteSafe(AddSelectedProductToCart);
            tabCatalog.Controls.Add(btnAddToCart);

            btnAddProduct = new Button { Left = 230, Top = 575, Width = 160, Height = 38, Text = "Добавить товар" };
            btnAddProduct.Click += (_, __) => ExecuteSafe(() => AddOrEditProduct());
            tabCatalog.Controls.Add(btnAddProduct);

            btnEditProduct = new Button { Left = 400, Top = 575, Width = 160, Height = 38, Text = "Изменить товар" };
            btnEditProduct.Click += (_, __) => ExecuteSafe(() => AddOrEditProduct(GetSelectedProduct()));
            tabCatalog.Controls.Add(btnEditProduct);

            btnDeleteProduct = new Button { Left = 570, Top = 575, Width = 160, Height = 38, Text = "Удалить товар" };
            btnDeleteProduct.Click += (_, __) => ExecuteSafe(DeleteSelectedProduct);
            tabCatalog.Controls.Add(btnDeleteProduct);
        }

        private void BuildCartTab()
        {
            dgvCart = new DataGridView
            {
                Left = 20,
                Top = 20,
                Width = 700,
                Height = 520,
                AutoGenerateColumns = true,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            tabCart.Controls.Add(dgvCart);

            BuildBookingPanel();

            btnRemoveFromCart = new Button { Left = 20, Top = 560, Width = 210, Height = 38, Text = "Удалить из корзины" };
            btnRemoveFromCart.Click += (_, __) => ExecuteSafe(RemoveSelectedCartItem);
            tabCart.Controls.Add(btnRemoveFromCart);

            btnCheckout = new Button { Left = 245, Top = 560, Width = 210, Height = 38, Text = "Оформить заказ" };
            btnCheckout.Click += (_, __) => ExecuteSafe(Checkout);
            tabCart.Controls.Add(btnCheckout);
        }

        private void BuildBookingPanel()
        {
            pnlBooking = new Panel
            {
                Left = 740,
                Top = 20,
                Width = 400,
                Height = 578,
                BackColor = Color.FromArgb(22, 27, 37),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            tabCart.Controls.Add(pnlBooking);

            var lblCalendar = new Label
            {
                Left = 20,
                Top = 16,
                Width = 250,
                Text = "Календарь",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(236, 239, 245)
            };
            pnlBooking.Controls.Add(lblCalendar);

            calBookingDate = new MonthCalendar
            {
                Left = 20,
                Top = 52,
                MaxSelectionCount = 1,
                FirstDayOfWeek = Day.Monday,
                MinDate = DateTime.Today,
                BackColor = Color.FromArgb(22, 27, 37),
                ForeColor = Color.FromArgb(225, 228, 235),
                TitleBackColor = Color.FromArgb(22, 27, 37),
                TitleForeColor = Color.FromArgb(225, 228, 235),
                TrailingForeColor = Color.FromArgb(102, 109, 125)
            };
            pnlBooking.Controls.Add(calBookingDate);

            var lblTime = new Label
            {
                Left = 20,
                Top = 230,
                Width = 180,
                Text = "Выберите время",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(236, 239, 245)
            };
            pnlBooking.Controls.Add(lblTime);

            cmbBookingHour = new ComboBox
            {
                Left = 20,
                Top = 257,
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbBookingHour.Items.AddRange(new object[]
            {
                "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00"
            });
            cmbBookingHour.SelectedIndex = 2;
            pnlBooking.Controls.Add(cmbBookingHour);

            var lblDuration = new Label
            {
                Left = 180,
                Top = 230,
                Width = 180,
                Text = "Длительность",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(236, 239, 245)
            };
            pnlBooking.Controls.Add(lblDuration);

            cmbDurationHours = new ComboBox
            {
                Left = 180,
                Top = 257,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDurationHours.Items.AddRange(new object[] { "2 часа", "4 часа", "8 часов", "12 часов" });
            cmbDurationHours.SelectedIndex = 3;
            cmbDurationHours.SelectedIndexChanged += (_, __) => UpdateQueueInfo();
            pnlBooking.Controls.Add(cmbDurationHours);

            var pnlQueue = new Panel
            {
                Left = 20,
                Top = 310,
                Width = 360,
                Height = 180,
                BackColor = Color.FromArgb(43, 49, 59)
            };
            pnlBooking.Controls.Add(pnlQueue);

            var lblQueueTitle = new Label
            {
                Left = 12,
                Top = 12,
                Width = 330,
                Text = "🔴 ОЧЕРЕДЬ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 89, 151)
            };
            pnlQueue.Controls.Add(lblQueueTitle);

            lblQueueHint = new Label
            {
                Left = 12,
                Top = 42,
                Width = 332,
                Height = 66,
                Text = "Вы можете встать в очередь. За 72 часа до выбранного времени сообщим подтверждение.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(195, 200, 212)
            };
            pnlQueue.Controls.Add(lblQueueHint);

            chkQueueBooking = new CheckBox
            {
                Left = 12,
                Top = 118,
                Width = 140,
                Text = "Записаться в очередь",
                ForeColor = Color.FromArgb(236, 239, 245),
                Checked = true
            };
            chkQueueBooking.CheckedChanged += (_, __) => UpdateQueueInfo();
            pnlQueue.Controls.Add(chkQueueBooking);

            lblQueuePrice = new Label
            {
                Left = 160,
                Top = 118,
                Width = 188,
                Height = 42,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 224, 108),
                TextAlign = ContentAlignment.TopRight
            };
            pnlQueue.Controls.Add(lblQueuePrice);
        }

        private void BuildOrdersTab()
        {
            dgvOrders = new DataGridView
            {
                Left = 20,
                Top = 20,
                Width = 1120,
                Height = 580,
                AutoGenerateColumns = true,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            tabOrders.Controls.Add(dgvOrders);
        }

        private void BuildUsersTab()
        {
            dgvUsers = new DataGridView
            {
                Left = 20,
                Top = 20,
                Width = 1120,
                Height = 510,
                AutoGenerateColumns = true,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            tabUsers.Controls.Add(dgvUsers);

            btnPromoteToManager = new Button { Left = 20, Top = 550, Width = 220, Height = 38, Text = "Назначить менеджером" };
            btnPromoteToManager.Click += (_, __) => ExecuteSafe(() => ChangeRole(UserRole.Manager));
            tabUsers.Controls.Add(btnPromoteToManager);

            btnPromoteToAdmin = new Button { Left = 250, Top = 550, Width = 220, Height = 38, Text = "Назначить администратором" };
            btnPromoteToAdmin.Click += (_, __) => ExecuteSafe(() => ChangeRole(UserRole.Administrator));
            tabUsers.Controls.Add(btnPromoteToAdmin);
        }

        private void Login()
        {
            _currentUser = _authService.Login(txtLogin.Text.Trim(), txtPassword.Text);
            lblStatus.Text = $"Вход выполнен: {_currentUser.FullName} ({_currentUser.Role})";
            FillProfile();
            ApplyRoleVisibility();
            RefreshAll();
        }

        private void Register()
        {
            var loginInput = txtLogin.Text.Trim();
            var email = txtEmailProfile.Text.Trim();
            var phone = txtPhoneProfile.Text.Trim();

            if (loginInput.Contains("@"))
                email = loginInput;
            else if (!string.IsNullOrWhiteSpace(loginInput))
                phone = loginInput;

            var newUser = new User
            {
                FullName = txtNameProfile.Text.Trim(),
                Email = email,
                Phone = phone,
                Password = txtPassword.Text
            };

            _authService.Register(newUser);
            MessageBox.Show("Регистрация успешна. Теперь войдите в систему.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveProfile()
        {
            EnsureAuthenticated();
            _authService.UpdateProfile(_currentUser, txtNameProfile.Text, txtEmailProfile.Text, txtPhoneProfile.Text, txtPasswordProfile.Text);
            lblStatus.Text = $"Профиль обновлён: {_currentUser.FullName} ({_currentUser.Role})";
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshCatalog();
            RefreshCart();
            RefreshOrders();
            RefreshUsers();
        }

        private void RefreshCatalog()
        {
            var sort = cmbSort != null ? cmbSort.SelectedItem?.ToString() : "Без сортировки";
            _catalogBinding.DataSource = _catalogService.QueryProducts(txtSearch?.Text, sort)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    Цена = p.Price,
                    СтараяЦена = p.OldPrice,
                    Скидка = p.DiscountPercent,
                    ИтоговаяЦена = p.EffectivePrice,
                    Изображение = p.ImagePath,
                    Premium = p.EffectivePrice > 1000 ? "★" : string.Empty
                }).ToList();

            if (dgvCatalog != null)
            {
                dgvCatalog.DataSource = _catalogBinding;
            }
        }

        private void HighlightPremiumProducts()
        {
            foreach (DataGridViewRow row in dgvCatalog.Rows)
            {
                if (row.Cells["ИтоговаяЦена"].Value is decimal price && price > 1000)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                    row.DefaultCellStyle.Font = new Font(dgvCatalog.Font, FontStyle.Bold);
                }

                if (row.Cells["СтараяЦена"].Value != null && row.Cells["Скидка"].Value != null)
                {
                    row.Cells["СтараяЦена"].Style.Font = new Font(dgvCatalog.Font, FontStyle.Strikeout);
                    row.Cells["СтараяЦена"].Style.ForeColor = Color.DarkRed;
                }
            }
        }

        private void AddSelectedProductToCart()
        {
            EnsureAuthenticated();
            var product = GetSelectedProduct();
            if (product == null) throw new InvalidOperationException("Выберите товар в каталоге.");

            var existing = _cartItems.FirstOrDefault(i => i.ProductId == product.Id);
            if (existing == null)
            {
                _cartItems.Add(new OrderItem { ProductId = product.Id, ProductName = product.Name, Quantity = 1, UnitPrice = product.EffectivePrice });
            }
            else
            {
                existing.Quantity++;
            }

            RefreshCart();
            tabMain.SelectedTab = tabCart;
        }

        private void RemoveSelectedCartItem()
        {
            EnsureAuthenticated();
            if (!(dgvCart.CurrentRow?.DataBoundItem is OrderItem item))
                throw new InvalidOperationException("Выберите позицию в корзине.");

            _cartItems.Remove(item);
            RefreshCart();
        }

        private void RefreshCart()
        {
            _cartBinding.DataSource = _cartItems.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();
            if (dgvCart != null) dgvCart.DataSource = _cartBinding;
            UpdateQueueInfo();
        }

        private void Checkout()
        {
            EnsureAuthenticated();
            var selectedDate = calBookingDate.SelectionStart.Date;
            var selectedHour = TimeSpan.Parse(cmbBookingHour.SelectedItem?.ToString() ?? "12:00");
            var scheduledAt = selectedDate.Add(selectedHour);
            var durationHours = ParseDurationHours();
            var useQueue = chkQueueBooking.Checked;

            _catalogService.Checkout(_currentUser.Id, _cartItems.ToList(), scheduledAt, durationHours, useQueue);
            _cartItems.Clear();
            RefreshCart();
            RefreshOrders();

            var bookingType = useQueue ? "очередь" : "мгновенное подтверждение";
            MessageBox.Show(
                $"Заказ успешно оформлен.\nДата: {scheduledAt:dd.MM.yyyy HH:mm}\nДлительность: {durationHours} ч.\nТип: {bookingType}.",
                "Запись подтверждена",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void RefreshOrders()
        {
            var orders = _catalogService.Orders;
            if (_currentUser != null && _currentUser.Role == UserRole.User)
                orders = orders.Where(o => o.UserId == _currentUser.Id).ToList();

            _ordersBinding.DataSource = orders.Select(o => new
            {
                o.Id,
                o.UserId,
                o.CreatedAt,
                ЗаписьНа = o.ScheduledAt,
                ДлительностьЧасов = o.DurationHours,
                Очередь = o.IsQueueBooking ? "Да" : "Нет",
                Депозит = o.QueueDepositAmount,
                o.TotalAmount,
                Positions = o.Items.Count
            }).ToList();

            if (dgvOrders != null) dgvOrders.DataSource = _ordersBinding;
        }

        private int ParseDurationHours()
        {
            var text = cmbDurationHours.SelectedItem?.ToString() ?? "12 часов";
            var digits = new string(text.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var value) ? value : 12;
        }

        private void UpdateQueueInfo()
        {
            if (lblQueuePrice == null)
                return;

            var duration = ParseDurationHours();
            var total = _cartItems.Sum(i => i.Total);
            var queueDeposit = Math.Round(total * 0.08m, 2);
            var perHour = duration > 0 ? Math.Round(queueDeposit / duration, 2) : queueDeposit;

            lblQueuePrice.Text = $"≈{queueDeposit:0.##} ₽ / {duration}ч\n≈{perHour:0.##} ₽ / 1ч";
            lblQueuePrice.ForeColor = chkQueueBooking != null && chkQueueBooking.Checked
                ? Color.FromArgb(41, 224, 108)
                : Color.FromArgb(138, 146, 165);
            lblQueueHint.ForeColor = chkQueueBooking != null && chkQueueBooking.Checked
                ? Color.FromArgb(195, 200, 212)
                : Color.FromArgb(138, 146, 165);
        }

        private void RefreshUsers()
        {
            _usersBinding.DataSource = _authService.Users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Role
            }).ToList();

            if (dgvUsers != null) dgvUsers.DataSource = _usersBinding;
        }

        private void AddOrEditProduct(Product source = null)
        {
            EnsureManagerOrAdmin();
            using (var form = new ProductEditorForm(source, _catalogService))
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                if (source == null)
                    _catalogService.AddProduct(form.Product);
                else
                    _catalogService.UpdateProduct(form.Product);

                RefreshCatalog();
            }
        }

        private void DeleteSelectedProduct()
        {
            EnsureAdministrator();
            var product = GetSelectedProduct();
            if (product == null) throw new InvalidOperationException("Выберите товар для удаления.");
            _catalogService.DeleteProduct(product.Id);
            RefreshCatalog();
        }

        private Product GetSelectedProduct()
        {
            if (dgvCatalog.CurrentRow == null) return null;
            var idObj = dgvCatalog.CurrentRow.Cells["Id"].Value;
            if (idObj == null) return null;
            var id = Convert.ToInt32(idObj);
            return _catalogService.Products.FirstOrDefault(p => p.Id == id);
        }

        private void ChangeRole(UserRole role)
        {
            EnsureAdministrator();
            if (dgvUsers.CurrentRow == null) throw new InvalidOperationException("Выберите пользователя.");
            var userId = Convert.ToInt32(dgvUsers.CurrentRow.Cells["Id"].Value);
            _authService.UpdateUserRole(userId, role);
            RefreshUsers();
        }

        private void FillProfile()
        {
            txtNameProfile.Text = _currentUser?.FullName ?? string.Empty;
            txtEmailProfile.Text = _currentUser?.Email ?? string.Empty;
            txtPhoneProfile.Text = _currentUser?.Phone ?? string.Empty;
            txtPasswordProfile.Text = string.Empty;
        }

        private void ExecuteSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Операция не выполнена", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyRoleVisibility()
        {
            var role = _currentUser?.Role ?? UserRole.Guest;
            var isAuthenticated = _currentUser != null;

            tabMain.Enabled = isAuthenticated;
            btnSaveProfile.Enabled = isAuthenticated;
            btnAddToCart.Enabled = isAuthenticated;
            btnRemoveFromCart.Enabled = isAuthenticated;
            btnCheckout.Enabled = isAuthenticated;

            var isManager = role == UserRole.Manager || role == UserRole.Administrator;
            var isAdmin = role == UserRole.Administrator;

            btnAddProduct.Enabled = isManager;
            btnEditProduct.Enabled = isManager;
            btnDeleteProduct.Enabled = isAdmin;

            tabUsers.Parent = isAdmin ? tabMain : null;
            if (!isAdmin && tabMain.TabPages.Contains(tabUsers))
                tabMain.TabPages.Remove(tabUsers);
            if (isAdmin && !tabMain.TabPages.Contains(tabUsers))
                tabMain.TabPages.Add(tabUsers);

            if (!isManager)
            {
                btnAddProduct.Enabled = false;
                btnEditProduct.Enabled = false;
                btnDeleteProduct.Enabled = false;
            }
        }

        private void EnsureAuthenticated()
        {
            if (_currentUser == null) throw new UnauthorizedAccessException("Требуется авторизация.");
        }

        private void EnsureManagerOrAdmin()
        {
            EnsureAuthenticated();
            if (_currentUser.Role != UserRole.Manager && _currentUser.Role != UserRole.Administrator)
                throw new UnauthorizedAccessException("Недостаточно прав. Доступно менеджеру/админу.");
        }

        private void EnsureAdministrator()
        {
            EnsureAuthenticated();
            if (_currentUser.Role != UserRole.Administrator)
                throw new UnauthorizedAccessException("Недостаточно прав. Доступно только администратору.");
        }
    }
}
