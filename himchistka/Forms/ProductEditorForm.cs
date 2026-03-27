using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using himchistka.Models;
using himchistka.Services;

namespace himchistka
{
    public sealed class ProductEditorForm : Form
    {
        private readonly CatalogService _catalogService;
        private readonly Product _source;

        private TextBox txtName;
        private TextBox txtCategory;
        private NumericUpDown numPrice;
        private NumericUpDown numOldPrice;
        private NumericUpDown numDiscount;
        private TextBox txtImage;

        public Product Product { get; private set; }

        public ProductEditorForm(Product source, CatalogService catalogService)
        {
            _source = source;
            _catalogService = catalogService;
            BuildUi();
            Fill();
        }

        private void BuildUi()
        {
            Text = _source == null ? "Добавление товара" : "Редактирование товара";
            Width = 560;
            Height = 370;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Controls.Add(new Label { Text = "Название", Left = 20, Top = 20, Width = 120 });
            txtName = new TextBox { Left = 150, Top = 16, Width = 360 };
            Controls.Add(txtName);

            Controls.Add(new Label { Text = "Категория", Left = 20, Top = 60, Width = 120 });
            txtCategory = new TextBox { Left = 150, Top = 56, Width = 360 };
            Controls.Add(txtCategory);

            Controls.Add(new Label { Text = "Цена", Left = 20, Top = 100, Width = 120 });
            numPrice = new NumericUpDown { Left = 150, Top = 96, Width = 160, DecimalPlaces = 2, Maximum = 100000, Minimum = 0 };
            Controls.Add(numPrice);

            Controls.Add(new Label { Text = "Старая цена", Left = 20, Top = 140, Width = 120 });
            numOldPrice = new NumericUpDown { Left = 150, Top = 136, Width = 160, DecimalPlaces = 2, Maximum = 100000, Minimum = 0 };
            Controls.Add(numOldPrice);

            Controls.Add(new Label { Text = "Скидка (%)", Left = 20, Top = 180, Width = 120 });
            numDiscount = new NumericUpDown { Left = 150, Top = 176, Width = 160, Maximum = 99, Minimum = 0 };
            Controls.Add(numDiscount);

            Controls.Add(new Label { Text = "Изображение", Left = 20, Top = 220, Width = 120 });
            txtImage = new TextBox { Left = 150, Top = 216, Width = 280, ReadOnly = true };
            Controls.Add(txtImage);

            var btnSelectImage = new Button { Left = 440, Top = 214, Width = 70, Height = 28, Text = "..." };
            btnSelectImage.Click += (_, __) => SelectImage();
            Controls.Add(btnSelectImage);

            var btnSave = new Button { Left = 150, Top = 270, Width = 170, Height = 35, Text = "Сохранить" };
            btnSave.Click += (_, __) => SaveAndClose();
            Controls.Add(btnSave);

            var btnCancel = new Button { Left = 340, Top = 270, Width = 170, Height = 35, Text = "Отмена" };
            btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);
        }

        private void Fill()
        {
            if (_source == null) return;
            txtName.Text = _source.Name;
            txtCategory.Text = _source.Category;
            numPrice.Value = _source.Price;
            numOldPrice.Value = _source.OldPrice ?? 0;
            numDiscount.Value = _source.DiscountPercent ?? 0;
            txtImage.Text = _source.ImagePath ?? string.Empty;
        }

        private void SelectImage()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Выберите изображение";
                dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                txtImage.Text = _catalogService.SaveImageToProject(dialog.FileName);
            }
        }

        private void SaveAndClose()
        {
            Product = new Product
            {
                Id = _source?.Id ?? 0,
                Name = txtName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                Price = numPrice.Value,
                OldPrice = numOldPrice.Value > 0 ? numOldPrice.Value : (decimal?)null,
                DiscountPercent = numDiscount.Value > 0 ? Convert.ToInt32(numDiscount.Value) : (int?)null,
                ImagePath = txtImage.Text.Trim()
            };

            ValidationService.ValidateProduct(Product);
            DialogResult = DialogResult.OK;
        }
    }
}
