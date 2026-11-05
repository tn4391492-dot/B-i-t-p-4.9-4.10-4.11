using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LabEquipmentCheckout
{
    public partial class Form1 : Form
    {
        // Controls
        private TextBox txtMaPhieu, txtMaSV, txtHoTen, txtTimKiem;
        private ComboBox cboThietBi, cboLoc;
        private NumericUpDown numSoLuong;
        private DateTimePicker dtpNgayMuon, dtpHanTra, dtpNgayTraThucTe;
        private CheckBox chkDaTra;
        private Button btnThem, btnCapNhat, btnXoa, btnLamMoi, btnXuatCSV;
        private DataGridView dgvPhieuMuon;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblTongSoPhieu, lblSoPhieuQuaHan;
        private ErrorProvider errorProvider;

        // Data sources
        private BindingList<BorrowRecord> masterList;
        private BindingList<BorrowRecord> displayList;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeComponentManual();
            InitData();
            CapNhatTrangThaiVaThongKe();
        }

        private void InitializeComponentManual()
        {
            this.Text = "Quản lý mượn trả thiết bị phòng thực hành - Form1";
            this.Size = new Size(1000, 720);
            this.MinimumSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            errorProvider = new ErrorProvider();

            // Root Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // 1. GroupBox Thông tin phiếu mượn
            GroupBox gbThongTin = new GroupBox
            {
                Text = "Thông tin phiếu mượn",
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            TableLayoutPanel inputGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 4,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            // Cấu hình cột tránh tràn/đè giao diện ngang
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));          // Cột 0: Nhãn 1
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));      // Cột 1: Control 1
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));          // Cột 2: Nhãn 2
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));      // Cột 3: Control 2

            // Thiết lập chiều cao cố định cho 5 dòng để tránh lấp ô dọc
            for (int i = 0; i < 5; i++)
            {
                inputGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            }

            // Controls
            txtMaPhieu = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtMaSV = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtHoTen = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };

            cboThietBi = new ComboBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboThietBi.Items.AddRange(new string[] { "Kính hiển vi", "Máy dao động ký", "Bộ thực hành vi điều khiển", "Nguồn xoay chiều DC", "Đồng hồ vạn năng" });

            numSoLuong = new NumericUpDown
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Minimum = 1,
                Maximum = 100,
                Value = 1
            };

            dtpNgayMuon = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            dtpHanTra = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            dtpNgayTraThucTe = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short, Enabled = false };

            chkDaTra = new CheckBox { Text = "Xác nhận đã trả", Anchor = AnchorStyles.Left, AutoSize = true };
            chkDaTra.CheckedChanged += ChkDaTra_CheckedChanged;

            // Thêm controls vào grid nhập liệu
            inputGrid.Controls.Add(new Label { Text = "Mã phiếu:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 10, 3) }, 0, 0);
            inputGrid.Controls.Add(txtMaPhieu, 1, 0);
            inputGrid.Controls.Add(new Label { Text = "Mã sinh viên:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(20, 6, 10, 3) }, 2, 0);
            inputGrid.Controls.Add(txtMaSV, 3, 0);

            inputGrid.Controls.Add(new Label { Text = "Họ tên SV:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 10, 3) }, 0, 1);
            inputGrid.Controls.Add(txtHoTen, 1, 1);
            inputGrid.Controls.Add(new Label { Text = "Thiết bị:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(20, 6, 10, 3) }, 2, 1);
            inputGrid.Controls.Add(cboThietBi, 3, 1);

            inputGrid.Controls.Add(new Label { Text = "Số lượng:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 10, 3) }, 0, 2);
            inputGrid.Controls.Add(numSoLuong, 1, 2);
            inputGrid.Controls.Add(new Label { Text = "Ngày mượn:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(20, 6, 10, 3) }, 2, 2);
            inputGrid.Controls.Add(dtpNgayMuon, 3, 2);

            inputGrid.Controls.Add(new Label { Text = "Hạn trả:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 6, 10, 3) }, 0, 3);
            inputGrid.Controls.Add(dtpHanTra, 1, 3);
            inputGrid.Controls.Add(new Label { Text = "Ngày trả thực:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(20, 6, 10, 3) }, 2, 3);
            inputGrid.Controls.Add(dtpNgayTraThucTe, 3, 3);

            inputGrid.Controls.Add(chkDaTra, 1, 4);

            // FlowLayoutPanel cho Button chức năng (Đã sửa lỗi cú pháp ở đây)
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            btnThem = new Button { Text = "Thêm", Width = 80, Height = 30 };
            btnCapNhat = new Button { Text = "Cập nhật", Width = 80, Height = 30 };
            btnXoa = new Button { Text = "Xóa", Width = 80, Height = 30 };
            btnLamMoi = new Button { Text = "Làm mới", Width = 80, Height = 30 };

            btnThem.Click += BtnThem_Click;
            btnCapNhat.Click += BtnCapNhat_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearInputs();

            pnlButtons.Controls.AddRange(new Control[] { btnThem, btnCapNhat, btnXoa, btnLamMoi });
            inputGrid.Controls.Add(pnlButtons, 3, 4);

            gbThongTin.Controls.Add(inputGrid);

            // 2. Khu vực Tìm kiếm & Lọc
            Panel pnlFilter = new Panel { Dock = DockStyle.Fill, Height = 40, Padding = new Padding(0, 5, 0, 5) };

            Label lblTim = new Label { Text = "Tìm kiếm (Mã SV / Thiết bị):", Location = new Point(5, 10), AutoSize = true };
            txtTimKiem = new TextBox { Location = new Point(175, 7), Width = 170 };
            txtTimKiem.TextChanged += FilterData;

            Label lblLoc = new Label { Text = "Lọc trạng thái:", Location = new Point(360, 10), AutoSize = true };
            cboLoc = new ComboBox { Location = new Point(450, 7), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoc.Items.AddRange(new string[] { "Tất cả", "Đang mượn", "Đã trả", "Quá hạn" });
            cboLoc.SelectedIndex = 0;
            cboLoc.SelectedIndexChanged += FilterData;

            btnXuatCSV = new Button { Text = "Xuất CSV", Location = new Point(600, 5), Width = 90, Height = 28 };
            btnXuatCSV.Click += BtnXuatCSV_Click;

            pnlFilter.Controls.AddRange(new Control[] { lblTim, txtTimKiem, lblLoc, cboLoc, btnXuatCSV });

            // 3. DataGridView Danh sách
            dgvPhieuMuon = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            ConfigureGridColumns();
            dgvPhieuMuon.SelectionChanged += DgvPhieuMuon_SelectionChanged;

            // StatusStrip
            statusStrip = new StatusStrip();
            lblTongSoPhieu = new ToolStripStatusLabel("Tổng số phiếu: 0");
            lblSoPhieuQuaHan = new ToolStripStatusLabel("Số phiếu quá hạn: 0") { ForeColor = Color.Red };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblTongSoPhieu, new ToolStripSeparator(), lblSoPhieuQuaHan });

            // Thêm các thành phần chính vào mainLayout
            mainLayout.Controls.Add(gbThongTin, 0, 0);
            mainLayout.Controls.Add(pnlFilter, 0, 1);
            mainLayout.Controls.Add(dgvPhieuMuon, 0, 2);

            this.Controls.Add(mainLayout);
            this.Controls.Add(statusStrip);
        }

        private void ConfigureGridColumns()
        {
            dgvPhieuMuon.Columns.Clear();
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MaPhieu", HeaderText = "Mã Phiếu", Width = 90 });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MaSinhVien", HeaderText = "Mã SV", Width = 90 });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HoTenSinhVien", HeaderText = "Họ Tên SV", Width = 140 });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ThietBi", HeaderText = "Thiết Bị", Width = 130 });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SoLuong", HeaderText = "Số Lượng", Width = 70 });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NgayMuon", HeaderText = "Ngày Mượn", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HanTra", HeaderText = "Hạn Trả", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NgayTraThucTe", HeaderText = "Ngày Trả Thực", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvPhieuMuon.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TinhTrangHienThi", HeaderText = "Tình Trạng", Width = 100 });
        }

        private void InitData()
        {
            masterList = new BindingList<BorrowRecord>();
            displayList = new BindingList<BorrowRecord>();
            bindingSource = new BindingSource { DataSource = displayList };
            dgvPhieuMuon.DataSource = bindingSource;

            FilterData(null, null);
        }

        private void ChkDaTra_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDaTra.Checked)
            {
                dtpNgayTraThucTe.Enabled = true;
                dtpNgayTraThucTe.Value = DateTime.Now;
            }
            else
            {
                dtpNgayTraThucTe.Enabled = false;
            }
        }

        private bool ValidateInput(bool isEdit = false)
        {
            errorProvider.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtMaPhieu.Text))
            {
                errorProvider.SetError(txtMaPhieu, "Mã phiếu không được để trống!");
                isValid = false;
            }
            else if (!isEdit && masterList.Any(p => p.MaPhieu.Equals(txtMaPhieu.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtMaPhieu, "Mã phiếu đã tồn tại!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                errorProvider.SetError(txtMaSV, "Mã sinh viên không được để trống!");
                isValid = false;
            }

            if (cboThietBi.SelectedIndex < 0)
            {
                errorProvider.SetError(cboThietBi, "Vui lòng chọn thiết bị!");
                isValid = false;
            }

            if (numSoLuong.Value <= 0)
            {
                errorProvider.SetError(numSoLuong, "Số lượng phải lớn hơn 0!");
                isValid = false;
            }

            if (dtpHanTra.Value.Date < dtpNgayMuon.Value.Date)
            {
                errorProvider.SetError(dtpHanTra, "Hạn trả không được trước ngày mượn!");
                isValid = false;
            }

            if (chkDaTra.Checked && dtpNgayTraThucTe.Value.Date < dtpNgayMuon.Value.Date)
            {
                errorProvider.SetError(dtpNgayTraThucTe, "Ngày trả thực tế không được trước ngày mượn!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(isEdit: false)) return;

            BorrowRecord record = new BorrowRecord
            {
                MaPhieu = txtMaPhieu.Text.Trim(),
                MaSinhVien = txtMaSV.Text.Trim(),
                HoTenSinhVien = txtHoTen.Text.Trim(),
                ThietBi = cboThietBi.SelectedItem.ToString(),
                SoLuong = (int)numSoLuong.Value,
                NgayMuon = dtpNgayMuon.Value.Date,
                HanTra = dtpHanTra.Value.Date,
                NgayTraThucTe = chkDaTra.Checked ? dtpNgayTraThucTe.Value.Date : (DateTime?)null,
                TinhTrang = chkDaTra.Checked ? BorrowStatus.DaTra : BorrowStatus.DangMuon
            };

            masterList.Add(record);
            FilterData(null, null);
            ClearInputs();
            MessageBox.Show("Thêm phiếu mượn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCapNhat_Click(object sender, EventArgs e)
        {
            if (dgvPhieuMuon.CurrentRow == null) return;
            if (!ValidateInput(isEdit: true)) return;

            string maPhieu = txtMaPhieu.Text.Trim();
            BorrowRecord record = masterList.FirstOrDefault(p => p.MaPhieu == maPhieu);

            if (record != null)
            {
                record.MaSinhVien = txtMaSV.Text.Trim();
                record.HoTenSinhVien = txtHoTen.Text.Trim();
                record.ThietBi = cboThietBi.SelectedItem.ToString();
                record.SoLuong = (int)numSoLuong.Value;
                record.NgayMuon = dtpNgayMuon.Value.Date;
                record.HanTra = dtpHanTra.Value.Date;
                record.NgayTraThucTe = chkDaTra.Checked ? dtpNgayTraThucTe.Value.Date : (DateTime?)null;

                if (chkDaTra.Checked)
                    record.TinhTrang = BorrowStatus.DaTra;
                else
                    record.TinhTrang = BorrowStatus.DangMuon;

                FilterData(null, null);
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvPhieuMuon.CurrentRow == null) return;

            string maPhieu = dgvPhieuMuon.CurrentRow.Cells[0].Value.ToString();
            DialogResult result = MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu {maPhieu}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                BorrowRecord record = masterList.FirstOrDefault(p => p.MaPhieu == maPhieu);
                if (record != null)
                {
                    masterList.Remove(record);
                    FilterData(null, null);
                    ClearInputs();
                }
            }
        }

        private void FilterData(object sender, EventArgs e)
        {
            CapNhatTrangThaiVaThongKe();

            string keyword = txtTimKiem.Text.Trim().ToLower();
            string statusFilter = cboLoc.SelectedItem?.ToString() ?? "Tất cả";

            var query = masterList.AsEnumerable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.MaSinhVien.ToLower().Contains(keyword) || p.ThietBi.ToLower().Contains(keyword));
            }

            if (statusFilter == "Đang mượn")
                query = query.Where(p => p.TinhTrang == BorrowStatus.DangMuon);
            else if (statusFilter == "Đã trả")
                query = query.Where(p => p.TinhTrang == BorrowStatus.DaTra);
            else if (statusFilter == "Quá hạn")
                query = query.Where(p => p.TinhTrang == BorrowStatus.QuaHan);

            displayList.Clear();
            foreach (var item in query)
            {
                displayList.Add(item);
            }
        }

        private void CapNhatTrangThaiVaThongKe()
        {
            int countQuaHan = 0;
            DateTime today = DateTime.Now.Date;

            foreach (var item in masterList)
            {
                if (item.TinhTrang != BorrowStatus.DaTra)
                {
                    if (item.HanTra < today)
                    {
                        item.TinhTrang = BorrowStatus.QuaHan;
                        item.TinhTrangHienThi = "Quá hạn";
                    }
                    else
                    {
                        item.TinhTrang = BorrowStatus.DangMuon;
                        item.TinhTrangHienThi = "Đang mượn";
                    }
                }
                else
                {
                    item.TinhTrangHienThi = "Đã trả";
                }

                if (item.TinhTrang == BorrowStatus.QuaHan)
                {
                    countQuaHan++;
                }
            }

            lblTongSoPhieu.Text = $"Tổng số phiếu: {masterList.Count}";
            lblSoPhieuQuaHan.Text = $"Số phiếu quá hạn: {countQuaHan}";
        }

        private void DgvPhieuMuon_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPhieuMuon.CurrentRow != null && dgvPhieuMuon.CurrentRow.Index < displayList.Count)
            {
                BorrowRecord record = (BorrowRecord)dgvPhieuMuon.CurrentRow.DataBoundItem;
                txtMaPhieu.Text = record.MaPhieu;
                txtMaPhieu.ReadOnly = true;
                txtMaSV.Text = record.MaSinhVien;
                txtHoTen.Text = record.HoTenSinhVien;
                cboThietBi.SelectedItem = record.ThietBi;
                numSoLuong.Value = record.SoLuong;
                dtpNgayMuon.Value = record.NgayMuon;
                dtpHanTra.Value = record.HanTra;

                if (record.TinhTrang == BorrowStatus.DaTra)
                {
                    chkDaTra.Checked = true;
                    dtpNgayTraThucTe.Value = record.NgayTraThucTe ?? DateTime.Now;
                }
                else
                {
                    chkDaTra.Checked = false;
                }
            }
        }

        private void ClearInputs()
        {
            txtMaPhieu.ReadOnly = false;
            txtMaPhieu.Clear();
            txtMaSV.Clear();
            txtHoTen.Clear();
            if (cboThietBi.Items.Count > 0) cboThietBi.SelectedIndex = 0;
            numSoLuong.Value = 1;
            dtpNgayMuon.Value = DateTime.Now;
            dtpHanTra.Value = DateTime.Now.AddDays(7);
            chkDaTra.Checked = false;
            dgvPhieuMuon.ClearSelection();
            errorProvider.Clear();
        }

        private void BtnXuatCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "DanhSachMuonThietBi.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Mã Phiếu,Mã SV,Họ Tên SV,Thiết Bị,Số Lượng,Ngày Mượn,Hạn Trả,Ngày Trả Thực Tế,Tình Trạng");

                        foreach (var item in displayList)
                        {
                            string ngayTraStr = item.NgayTraThucTe.HasValue ? item.NgayTraThucTe.Value.ToString("dd/MM/yyyy") : "";
                            sb.AppendLine($"\"{item.MaPhieu}\",\"{item.MaSinhVien}\",\"{item.HoTenSinhVien}\",\"{item.ThietBi}\",{item.SoLuong},\"{item.NgayMuon:dd/MM/yyyy}\",\"{item.HanTra:dd/MM/yyyy}\",\"{ngayTraStr}\",\"{item.TinhTrangHienThi}\"");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}