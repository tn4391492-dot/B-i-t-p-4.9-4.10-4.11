using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace bt4_9
{
    public partial class Form1 : Form
    {
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuExportCSV;

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatusCount;

        private TableLayoutPanel mainLayout;
        private Panel panelInput;
        private Panel panelSearchFilter;
        private DataGridView dgvAppointments;

        private TextBox txtId, txtName, txtPhone, txtReason;
        private DateTimePicker dtpDateTime;
        private ComboBox cmbDoctor, cmbStatus;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;

        private TextBox txtSearch;
        private CheckBox chkFilterDate;
        private DateTimePicker dtpFilterDate;
        private ComboBox cmbFilterStatus;
        private Button btnResetFilter;

        private BindingList<Appointment> appointmentList;
        private BindingSource bindingSource;
        private ErrorProvider errorProvider1;

        public Form1()
        {
            InitializeComponent();
            InitializeComponentCustom();
            InitData();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Clinic Appointment Manager - Quản lý lịch hẹn phòng khám";
            this.Width = 1100;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;

            errorProvider1 = new ErrorProvider();

            menuStrip1 = new MenuStrip();
            menuFile = new ToolStripMenuItem("Hệ thống");
            menuExportCSV = new ToolStripMenuItem("Xuất danh sách ra CSV...", null, MenuExportCSV_Click);
            menuFile.DropDownItems.Add(menuExportCSV);
            menuStrip1.Items.Add(menuFile);
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);

            statusStrip1 = new StatusStrip();
            lblStatusCount = new ToolStripStatusLabel("Tổng số lịch hẹn: 0");
            statusStrip1.Items.Add(lblStatusCount);
            this.Controls.Add(statusStrip1);

            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainLayout);
            mainLayout.BringToFront();
            mainLayout.Padding = new Padding(10, 30, 10, 30);

            panelInput = new Panel();
            panelInput.Dock = DockStyle.Fill;

            TableLayoutPanel inputGrid = new TableLayoutPanel();
            inputGrid.Dock = DockStyle.Fill;
            inputGrid.ColumnCount = 4;
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            inputGrid.RowCount = 4;
            inputGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            inputGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            inputGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            inputGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            txtId = new TextBox { Dock = DockStyle.Fill, Enabled = false }; // Khóa không cho sửa trực tiếp mã tự sinh
            txtName = new TextBox { Dock = DockStyle.Fill };
            txtPhone = new TextBox { Dock = DockStyle.Fill };
            dtpDateTime = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };

            cmbDoctor = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDoctor.Items.AddRange(new string[] { "BS. Nguyễn Văn An", "BS. Trần Thị Bình", "BS. Lê Hoàng Long", "BS. Phạm Minh Thư" });
            cmbDoctor.SelectedIndex = 0;

            txtReason = new TextBox { Dock = DockStyle.Fill };

            cmbStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new string[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cmbStatus.SelectedIndex = 0;

            inputGrid.Controls.Add(new Label { Text = "Mã lịch hẹn:", Anchor = AnchorStyles.Left }, 0, 0);
            inputGrid.Controls.Add(txtId, 1, 0);
            inputGrid.Controls.Add(new Label { Text = "Bác sĩ:", Anchor = AnchorStyles.Left }, 2, 0);
            inputGrid.Controls.Add(cmbDoctor, 3, 0);

            inputGrid.Controls.Add(new Label { Text = "Họ tên BN:", Anchor = AnchorStyles.Left }, 0, 1);
            inputGrid.Controls.Add(txtName, 1, 1);
            inputGrid.Controls.Add(new Label { Text = "Lý do khám:", Anchor = AnchorStyles.Left }, 2, 1);
            inputGrid.Controls.Add(txtReason, 3, 1);

            inputGrid.Controls.Add(new Label { Text = "Số điện thoại:", Anchor = AnchorStyles.Left }, 0, 2);
            inputGrid.Controls.Add(txtPhone, 1, 2);
            inputGrid.Controls.Add(new Label { Text = "Trạng thái:", Anchor = AnchorStyles.Left }, 2, 2);
            inputGrid.Controls.Add(cmbStatus, 3, 2);

            inputGrid.Controls.Add(new Label { Text = "Ngày giờ hẹn:", Anchor = AnchorStyles.Left }, 0, 3);
            inputGrid.Controls.Add(dtpDateTime, 1, 3);

            FlowLayoutPanel panelButtons = new FlowLayoutPanel { Dock = DockStyle.Fill };
            btnAdd = new Button { Text = "Thêm mới", Width = 85 };
            btnUpdate = new Button { Text = "Cập nhật", Width = 85 };
            btnDelete = new Button { Text = "Xóa", Width = 75 };
            btnClear = new Button { Text = "Làm mới", Width = 75 };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            inputGrid.Controls.Add(panelButtons, 3, 3);
            panelInput.Controls.Add(inputGrid);
            mainLayout.Controls.Add(panelInput, 0, 0);

            panelSearchFilter = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };

            txtSearch = new TextBox { Width = 200 };
            txtSearch.TextChanged += ApplyFilterAndSearch;

            chkFilterDate = new CheckBox { Text = "Lọc ngày:", AutoSize = true, Checked = false };
            chkFilterDate.CheckedChanged += ApplyFilterAndSearch;

            dtpFilterDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
            dtpFilterDate.ValueChanged += ApplyFilterAndSearch;

            cmbFilterStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
            cmbFilterStatus.Items.Add("-- Tất cả trạng thái --");
            cmbFilterStatus.Items.AddRange(new string[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += ApplyFilterAndSearch;

            btnResetFilter = new Button { Text = "Xóa bộ lọc", Width = 90 };
            btnResetFilter.Click += BtnResetFilter_Click;

            panelSearchFilter.Controls.AddRange(new Control[] {
                new Label { Text = "Tìm kiếm:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) },
                txtSearch,
                chkFilterDate,
                dtpFilterDate,
                new Label { Text = "Trạng thái:", AutoSize = true, Padding = new Padding(10, 5, 0, 0) },
                cmbFilterStatus,
                btnResetFilter
            });
            mainLayout.Controls.Add(panelSearchFilter, 0, 1);

            dgvAppointments = new DataGridView();
            dgvAppointments.Dock = DockStyle.Fill;
            dgvAppointments.AutoGenerateColumns = false;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.MultiSelect = false;
            dgvAppointments.ReadOnly = true;
            dgvAppointments.AllowUserToAddRows = false;

            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Mã LH", Width = 80 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PatientName", HeaderText = "Họ tên bệnh nhân", Width = 160 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Số điện thoại", Width = 110 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentDateTime", HeaderText = "Ngày giờ hẹn", Width = 140, DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" } });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Doctor", HeaderText = "Bác sĩ", Width = 150 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Reason", HeaderText = "Lý do khám", Width = 180 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 110 });

            dgvAppointments.CellClick += DgvAppointments_CellClick;
            mainLayout.Controls.Add(dgvAppointments, 0, 2);
        }

        private void InitData()
        {
            appointmentList = new BindingList<Appointment>();

            bindingSource = new BindingSource();
            bindingSource.DataSource = appointmentList;
            dgvAppointments.DataSource = bindingSource;

            ClearForm(); // Tự động sinh mã đầu tiên khi khởi động
            UpdateStatusCount();
        }

        // Hàm tự động sinh mã lịch hẹn tăng dần dạng LH001, LH002...
        private string GenerateNewAppointmentId()
        {
            int nextIdNum = 1;
            if (appointmentList != null && appointmentList.Count > 0)
            {
                var numbers = appointmentList.Select(x =>
                {
                    string digits = new string(x.Id.Where(char.IsDigit).ToArray());
                    int.TryParse(digits, out int val);
                    return val;
                });

                if (numbers.Any())
                {
                    nextIdNum = numbers.Max() + 1;
                }
            }
            return $"LH{nextIdNum:D3}";
        }

        private bool ValidateInputs(bool isNew)
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                errorProvider1.SetError(txtId, "Mã lịch hẹn không được để trống!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Họ tên bệnh nhân không được để trống!");
                isValid = false;
            }

            string phonePattern = @"^0\d{9,10}$";
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                errorProvider1.SetError(txtPhone, "Số điện thoại không được để trống!");
                isValid = false;
            }
            else if (!Regex.IsMatch(txtPhone.Text.Trim(), phonePattern))
            {
                errorProvider1.SetError(txtPhone, "Số điện thoại không hợp lệ (Phải bắt đầu bằng số 0 và có 10-11 chữ số)!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                errorProvider1.SetError(txtReason, "Lý do khám không được để trống!");
                isValid = false;
            }

            if (dtpDateTime.Value < DateTime.Now)
            {
                errorProvider1.SetError(dtpDateTime, "Ngày giờ hẹn không được nằm trong quá khứ!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(true)) return;

            var newAppt = new Appointment
            {
                Id = txtId.Text.Trim(),
                PatientName = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                AppointmentDateTime = dtpDateTime.Value,
                Doctor = cmbDoctor.SelectedItem.ToString(),
                Reason = txtReason.Text.Trim(),
                Status = cmbStatus.SelectedItem.ToString()
            };

            appointmentList.Add(newAppt);
            ApplyFilterAndSearch(null, null);
            ClearForm(); // Tự động sinh mã tiếp theo cho lịch hẹn mới
            MessageBox.Show("Thêm lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần cập nhật từ danh sách!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInputs(false)) return;

            var currentAppt = bindingSource.Current as Appointment;
            if (currentAppt != null)
            {
                currentAppt.PatientName = txtName.Text.Trim();
                currentAppt.Phone = txtPhone.Text.Trim();
                currentAppt.AppointmentDateTime = dtpDateTime.Value;
                currentAppt.Doctor = cmbDoctor.SelectedItem.ToString();
                currentAppt.Reason = txtReason.Text.Trim();
                currentAppt.Status = cmbStatus.SelectedItem.ToString();

                bindingSource.ResetBindings(false);
                ApplyFilterAndSearch(null, null);
                ClearForm();
                MessageBox.Show("Cập nhật lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa lịch hẹn này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                var currentAppt = bindingSource.Current as Appointment;
                if (currentAppt != null)
                {
                    appointmentList.Remove(currentAppt);
                    ApplyFilterAndSearch(null, null);
                    ClearForm();
                    MessageBox.Show("Đã xóa lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            dgvAppointments.ClearSelection();
        }

        private void ClearForm()
        {
            txtId.Text = GenerateNewAppointmentId(); // Tự động sinh mã mới khi làm mới form
            txtName.Clear();
            txtPhone.Clear();
            txtReason.Clear();
            dtpDateTime.Value = DateTime.Now;
            if (cmbDoctor.Items.Count > 0) cmbDoctor.SelectedIndex = 0;
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;
            errorProvider1.Clear();
        }

        private void DgvAppointments_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var appt = dgvAppointments.Rows[e.RowIndex].DataBoundItem as Appointment;
                if (appt != null)
                {
                    txtId.Text = appt.Id;
                    txtName.Text = appt.PatientName;
                    txtPhone.Text = appt.Phone;
                    dtpDateTime.Value = appt.AppointmentDateTime;
                    cmbDoctor.SelectedItem = appt.Doctor;
                    txtReason.Text = appt.Reason;
                    cmbStatus.SelectedItem = appt.Status;
                    errorProvider1.Clear();
                }
            }
        }

        private void ApplyFilterAndSearch(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            string selectedStatus = cmbFilterStatus.SelectedIndex > 0 ? cmbFilterStatus.SelectedItem.ToString() : null;
            bool filterByDate = chkFilterDate.Checked;
            DateTime targetDate = dtpFilterDate.Value.Date;

            var filtered = appointmentList.Where(x =>
            {
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                                    x.PatientName.ToLower().Contains(keyword) ||
                                    x.Phone.Contains(keyword);

                bool matchStatus = selectedStatus == null || x.Status == selectedStatus;

                bool matchDate = !filterByDate || x.AppointmentDateTime.Date == targetDate;

                return matchKeyword && matchStatus && matchDate;
            }).ToList();

            bindingSource.DataSource = new BindingList<Appointment>(filtered);
            UpdateStatusCount();
        }

        private void BtnResetFilter_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            chkFilterDate.Checked = false;
            cmbFilterStatus.SelectedIndex = 0;
            bindingSource.DataSource = appointmentList;
            UpdateStatusCount();
        }

        private void UpdateStatusCount()
        {
            int count = bindingSource.Count;
            lblStatusCount.Text = $"Số lịch hẹn đang hiển thị: {count}";
        }

        private void MenuExportCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.FileName = $"DanhSachLichHen_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("Mã Lịch Hẹn,Họ Tên Bệnh Nhân,Số Điện Thoại,Ngày Giờ Hẹn,Bác Sĩ,Lý Do Khám,Trạng Thái");

                            foreach (var item in bindingSource.DataSource as BindingList<Appointment> ?? appointmentList)
                            {
                                sw.WriteLine($"\"{item.Id}\",\"{item.PatientName}\",\"{item.Phone}\",\"{item.AppointmentDateTime:dd/MM/yyyy HH:mm}\",\"{item.Doctor}\",\"{item.Reason}\",\"{item.Status}\"");
                            }
                        }
                        MessageBox.Show("Xuất file CSV thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}