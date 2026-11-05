using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace bt4_11
{
    // Class chứa thông tin đơn đăng ký sửa chữa
    public class RepairOrder
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string DeviceType { get; set; }
        public string ErrorDescription { get; set; }
        public DateTime ReceptionDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
    }

    public partial class Form1 : Form
    {
        // Danh sách dữ liệu gốc và binding (Danh sách trống ban đầu)
        private BindingList<RepairOrder> _originalList = new BindingList<RepairOrder>();
        private BindingSource _bindingSource = new BindingSource();

        // Control giao diện chính
        private MenuStrip _menuStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatusCount;
        private ToolStripStatusLabel _lblTotalPendingCost;

        // Controls nhập liệu
        private TextBox _txtOrderId;
        private TextBox _txtCustomerName;
        private TextBox _txtPhone;
        private ComboBox _cboDeviceType;
        private TextBox _txtErrorDesc;
        private DateTimePicker _dtpReceptionDate;
        private TextBox _txtEstimatedCost;
        private ComboBox _cboStatus;
        private PictureBox _picDevice;
        private Button _btnBrowseImage;

        // Controls tìm kiếm & lọc
        private TextBox _txtSearch;
        private ComboBox _cboFilterStatus;

        // DataGridView
        private DataGridView _dgvOrders;

        // Buttons chức năng
        private Button _btnAdd;
        private Button _btnUpdate;
        private Button _btnDelete;
        private Button _btnClear;

        // ErrorProvider xử lý kiểm tra dữ liệu
        private ErrorProvider _errorProvider;

        public Form1()
        {
            InitializeComponentCustom();
            SetupDataGridView();
            BindControls();
            UpdateStatusStrip();
        }

        #region Khởi tạo Giao diện Responsive bằng Code

        private void InitializeComponentCustom()
        {
            this.Text = "Repair Service Tracker - Quản lý đơn sửa chữa";
            this.Size = new Size(1150, 720);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            _errorProvider = new ErrorProvider();

            // Top MenuStrip
            _menuStrip = new MenuStrip();
            var menuFile = new ToolStripMenuItem("Hệ thống");
            var menuExport = new ToolStripMenuItem("Xuất danh sách ra CSV", null, MenuExportCSV_Click);
            var menuExit = new ToolStripMenuItem("Thoát", null, (s, e) => this.Close());
            menuFile.DropDownItems.Add(menuExport);
            menuFile.DropDownItems.Add(new ToolStripSeparator());
            menuFile.DropDownItems.Add(menuExit);
            _menuStrip.Items.Add(menuFile);
            this.MainMenuStrip = _menuStrip;

            // Bottom StatusStrip
            _statusStrip = new StatusStrip();
            _lblStatusCount = new ToolStripStatusLabel("Số đơn: 0");
            _lblTotalPendingCost = new ToolStripStatusLabel("Tổng chi phí chưa hoàn tất: 0 VNĐ")
            {
                Alignment = ToolStripItemAlignment.Right
            };
            _statusStrip.Items.AddRange(new ToolStripItem[] { _lblStatusCount, _lblTotalPendingCost });

            // Main TableLayoutPanel Responsive
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F)); // Cột trái: Thông tin nhập
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Cột phải: DataGridView
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));        // Hàng 1: Filter/Search
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));       // Hàng 2: Nội dung chính

            // --- PANEL TÌM KIẾM & LỌC (Hàng 1, Cột 2) ---
            Panel pnlFilter = new Panel { Dock = DockStyle.Fill };
            Label lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, Location = new Point(5, 12) };
            _txtSearch = new TextBox { Location = new Point(70, 8), Width = 180 };
            _txtSearch.TextChanged += FilterData;

            Label lblFilterStatus = new Label { Text = "Lọc trạng thái:", AutoSize = true, Location = new Point(270, 12) };
            _cboFilterStatus = new ComboBox
            {
                Location = new Point(360, 8),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboFilterStatus.Items.AddRange(new object[] { "Tất cả", "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" });
            _cboFilterStatus.SelectedIndex = 0;
            _cboFilterStatus.SelectedIndexChanged += FilterData;

            pnlFilter.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblFilterStatus, _cboFilterStatus });
            mainLayout.Controls.Add(pnlFilter, 1, 0);

            // --- PANEL THÔNG TIN ĐƠN (Hàng 2, Cột 1) ---
            GroupBox grpInput = new GroupBox
            {
                Text = "Thông tin đơn sửa chữa",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            TableLayoutPanel inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                AutoScroll = true
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _txtOrderId = new TextBox { Width = 240 };
            _txtCustomerName = new TextBox { Width = 240 };
            _txtPhone = new TextBox { Width = 240 };

            _cboDeviceType = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboDeviceType.Items.AddRange(new object[] { "Điện thoại", "Laptop", "Máy tính bảng", "PC", "Thiết bị khác" });
            _cboDeviceType.SelectedIndex = 0;

            _txtErrorDesc = new TextBox { Width = 240, Multiline = true, Height = 55 };
            _dtpReceptionDate = new DateTimePicker { Width = 240, Format = DateTimePickerFormat.Short };
            _txtEstimatedCost = new TextBox { Width = 240, Text = "0" };

            _cboStatus = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboStatus.Items.AddRange(new object[] { "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" });
            _cboStatus.SelectedIndex = 0;

            // PictureBox chuẩn tỷ lệ (Zoom)
            _picDevice = new PictureBox
            {
                Size = new Size(130, 90),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            _btnBrowseImage = new Button { Text = "Chọn ảnh...", Width = 90 };
            _btnBrowseImage.Click += BtnBrowseImage_Click;

            FlowLayoutPanel pnlImage = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            pnlImage.Controls.Add(_picDevice);
            pnlImage.Controls.Add(_btnBrowseImage);

            // Button Action Panel
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            _btnAdd = new Button { Text = "Thêm", Width = 75 };
            _btnUpdate = new Button { Text = "Cập nhật", Width = 75 };
            _btnDelete = new Button { Text = "Xóa", Width = 75 };
            _btnClear = new Button { Text = "Làm mới", Width = 75 };

            _btnAdd.Click += BtnAdd_Click;
            _btnUpdate.Click += BtnUpdate_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnClear.Click += (s, e) => ClearForm();

            pnlButtons.Controls.AddRange(new Control[] { _btnAdd, _btnUpdate, _btnDelete, _btnClear });

            // Add Control vào Input Layout
            AddFormRow(inputLayout, 0, "Mã đơn (*):", _txtOrderId);
            AddFormRow(inputLayout, 1, "Tên khách (*):", _txtCustomerName);
            AddFormRow(inputLayout, 2, "Số điện thoại (*):", _txtPhone);
            AddFormRow(inputLayout, 3, "Loại thiết bị:", _cboDeviceType);
            AddFormRow(inputLayout, 4, "Mô tả lỗi (*):", _txtErrorDesc);
            AddFormRow(inputLayout, 5, "Ngày tiếp nhận:", _dtpReceptionDate);
            AddFormRow(inputLayout, 6, "Chi phí dự kiến:", _txtEstimatedCost);
            AddFormRow(inputLayout, 7, "Trạng thái:", _cboStatus);
            AddFormRow(inputLayout, 8, "Ảnh thiết bị:", pnlImage);

            inputLayout.Controls.Add(pnlButtons, 0, 9);
            inputLayout.SetColumnSpan(pnlButtons, 2);

            grpInput.Controls.Add(inputLayout);
            mainLayout.Controls.Add(grpInput, 0, 1);

            // --- DATAGRIDVIEW (Hàng 2, Cột 2) ---
            _dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

            mainLayout.Controls.Add(_dgvOrders, 1, 1);

            // Add Main Layout & Menu/Status vào Form
            this.Controls.Add(mainLayout);
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_menuStrip);
        }

        private void AddFormRow(TableLayoutPanel panel, int row, string labelText, Control ctrl)
        {
            Label lbl = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Padding = new Padding(0, 5, 0, 0) };
            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(ctrl, 1, row);
        }

        #endregion

        #region DataGridView & Binding Data

        private void SetupDataGridView()
        {
            _dgvOrders.Columns.Clear();

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = "Mã Đơn",
                Width = 90
            });
            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CustomerName",
                HeaderText = "Tên Khách Hàng",
                Width = 140
            });
            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Phone",
                HeaderText = "Số Điện Thoại",
                Width = 100
            });
            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DeviceType",
                HeaderText = "Loại Thiết Bị",
                Width = 110
            });
            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ErrorDescription",
                HeaderText = "Mô Tả Lỗi",
                Width = 150
            });
            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceptionDate",
                HeaderText = "Ngày Tiếp Nhận",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            // Định dạng chi phí theo dạng N0
            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EstimatedCost",
                HeaderText = "Chi Phí Dự Kiến",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Trạng Thái",
                Width = 110
            });
        }

        private void BindControls()
        {
            _bindingSource.DataSource = _originalList;
            _dgvOrders.DataSource = _bindingSource;
        }

        #endregion

        #region Chức năng Thêm, Sửa, Xóa, Validation & Filter

        private bool ValidateData(bool isUpdating = false)
        {
            _errorProvider.Clear();
            bool isValid = true;

            // Kiểm tra Mã đơn
            if (string.IsNullOrWhiteSpace(_txtOrderId.Text))
            {
                _errorProvider.SetError(_txtOrderId, "Mã đơn không được để trống!");
                isValid = false;
            }
            else if (!isUpdating && _originalList.Any(x => x.OrderId.Equals(_txtOrderId.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                _errorProvider.SetError(_txtOrderId, "Mã đơn đã tồn tại trong hệ thống!");
                isValid = false;
            }

            // Kiểm tra Tên khách hàng
            if (string.IsNullOrWhiteSpace(_txtCustomerName.Text))
            {
                _errorProvider.SetError(_txtCustomerName, "Tên khách hàng không được để trống!");
                isValid = false;
            }

            // Kiểm tra Số điện thoại
            if (string.IsNullOrWhiteSpace(_txtPhone.Text))
            {
                _errorProvider.SetError(_txtPhone, "Số điện thoại không được để trống!");
                isValid = false;
            }

            // Kiểm tra Mô tả lỗi
            if (string.IsNullOrWhiteSpace(_txtErrorDesc.Text))
            {
                _errorProvider.SetError(_txtErrorDesc, "Mô tả lỗi bắt buộc nhập!");
                isValid = false;
            }

            // Kiểm tra Chi phí dự kiến >= 0
            if (!decimal.TryParse(_txtEstimatedCost.Text, out decimal cost) || cost < 0)
            {
                _errorProvider.SetError(_txtEstimatedCost, "Chi phí phải là số hợp lệ không được âm!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateData(isUpdating: false)) return;

            var newOrder = new RepairOrder
            {
                OrderId = _txtOrderId.Text.Trim(),
                CustomerName = _txtCustomerName.Text.Trim(),
                Phone = _txtPhone.Text.Trim(),
                DeviceType = _cboDeviceType.SelectedItem?.ToString() ?? "Thiết bị khác",
                ErrorDescription = _txtErrorDesc.Text.Trim(),
                ReceptionDate = _dtpReceptionDate.Value,
                EstimatedCost = decimal.Parse(_txtEstimatedCost.Text),
                Status = _cboStatus.SelectedItem?.ToString() ?? "Mới tiếp nhận",
                ImagePath = _picDevice.Tag?.ToString()
            };

            _originalList.Add(newOrder);
            FilterData(null, null);
            ClearForm();
            MessageBox.Show("Thêm đơn đăng ký mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_dgvOrders.CurrentRow == null) return;
            if (!ValidateData(isUpdating: true)) return;

            var currentOrder = _dgvOrders.CurrentRow.DataBoundItem as RepairOrder;
            if (currentOrder != null)
            {
                currentOrder.CustomerName = _txtCustomerName.Text.Trim();
                currentOrder.Phone = _txtPhone.Text.Trim();
                currentOrder.DeviceType = _cboDeviceType.SelectedItem?.ToString();
                currentOrder.ErrorDescription = _txtErrorDesc.Text.Trim();
                currentOrder.ReceptionDate = _dtpReceptionDate.Value;
                currentOrder.EstimatedCost = decimal.Parse(_txtEstimatedCost.Text);
                currentOrder.Status = _cboStatus.SelectedItem?.ToString();
                currentOrder.ImagePath = _picDevice.Tag?.ToString();

                _bindingSource.ResetCurrentItem();
                UpdateStatusStrip();
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_dgvOrders.CurrentRow == null) return;

            var currentOrder = _dgvOrders.CurrentRow.DataBoundItem as RepairOrder;
            if (currentOrder != null)
            {
                var confirm = MessageBox.Show($"Bạn có chắc muốn xóa đơn [{currentOrder.OrderId}] không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    _originalList.Remove(currentOrder);
                    FilterData(null, null);
                    ClearForm();
                    MessageBox.Show("Đã xóa đơn sửa chữa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Chọn ảnh thiết bị";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _picDevice.Image = Image.FromFile(ofd.FileName);
                    _picDevice.Tag = ofd.FileName; // Lưu đường dẫn ảnh vào Tag
                }
            }
        }

        private void DgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgvOrders.CurrentRow != null && _dgvOrders.CurrentRow.DataBoundItem is RepairOrder item)
            {
                _txtOrderId.Text = item.OrderId;
                _txtOrderId.ReadOnly = true; // Không cho sửa Mã đơn đã chọn
                _txtCustomerName.Text = item.CustomerName;
                _txtPhone.Text = item.Phone;
                _cboDeviceType.SelectedItem = item.DeviceType;
                _txtErrorDesc.Text = item.ErrorDescription;
                _dtpReceptionDate.Value = item.ReceptionDate;
                _txtEstimatedCost.Text = item.EstimatedCost.ToString("G0");
                _cboStatus.SelectedItem = item.Status;

                // Load ảnh nếu có
                if (!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
                {
                    _picDevice.Image = Image.FromFile(item.ImagePath);
                    _picDevice.Tag = item.ImagePath;
                }
                else
                {
                    _picDevice.Image = null;
                    _picDevice.Tag = null;
                }
            }
        }

        private void ClearForm()
        {
            _txtOrderId.ReadOnly = false;
            _txtOrderId.Clear();
            _txtCustomerName.Clear();
            _txtPhone.Clear();
            _txtErrorDesc.Clear();
            _txtEstimatedCost.Text = "0";
            _cboDeviceType.SelectedIndex = 0;
            _cboStatus.SelectedIndex = 0;
            _dtpReceptionDate.Value = DateTime.Now;
            _picDevice.Image = null;
            _picDevice.Tag = null;
            _errorProvider.Clear();
        }

        // Tìm kiếm tức thời & Lọc trạng thái
        private void FilterData(object sender, EventArgs e)
        {
            string keyword = _txtSearch.Text.Trim().ToLower();
            string selectedStatus = _cboFilterStatus.SelectedItem?.ToString();

            var filtered = _originalList.Where(item =>
            {
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                                    item.OrderId.ToLower().Contains(keyword) ||
                                    item.CustomerName.ToLower().Contains(keyword) ||
                                    item.Phone.ToLower().Contains(keyword);

                bool matchStatus = selectedStatus == "Tất cả" || item.Status == selectedStatus;

                return matchKeyword && matchStatus;
            }).ToList();

            _bindingSource.DataSource = new BindingList<RepairOrder>(filtered);
            UpdateStatusStrip();
        }

        #endregion

        #region MenuStrip (Xuất CSV) & StatusStrip

        private void UpdateStatusStrip()
        {
            int count = _dgvOrders.Rows.Count;
            _lblStatusCount.Text = $"Số đơn theo bộ lọc: {count}";

            // Tính tổng chi phí dự kiến của các đơn chưa hoàn tất (Trạng thái != Hoàn tất & != Đã hủy)
            decimal pendingTotal = _originalList
                .Where(o => o.Status != "Hoàn tất" && o.Status != "Đã hủy")
                .Sum(o => o.EstimatedCost);

            _lblTotalPendingCost.Text = $"Tổng chi phí chưa hoàn tất: {pendingTotal:N0} VNĐ";
        }

        private void MenuExportCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv";
                sfd.FileName = $"RepairOrders_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        // Header
                        sb.AppendLine("MaDon,TenKhachHang,SoDienThoai,LoaiThietBi,MoTaLoi,NgayTiepNhan,ChiPhiDuKien,TrangThai");

                        // Data từ danh sách đang hiển thị trên DGV
                        foreach (DataGridViewRow row in _dgvOrders.Rows)
                        {
                            if (row.DataBoundItem is RepairOrder item)
                            {
                                string line = $"\"{item.OrderId}\",\"{item.CustomerName}\",\"{item.Phone}\",\"{item.DeviceType}\",\"{item.ErrorDescription.Replace("\"", "\"\"")}\",\"{item.ReceptionDate:dd/MM/yyyy}\",\"{item.EstimatedCost}\",\"{item.Status}\"";
                                sb.AppendLine(line);
                            }
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Xuất tập tin CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Có lỗi xảy ra khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion
    }
}