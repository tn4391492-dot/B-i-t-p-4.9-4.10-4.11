using System;

namespace LabEquipmentCheckout
{
    public enum BorrowStatus
    {
        DangMuon, // Đang mượn
        DaTra,    // Đã trả
        QuaHan    // Quá hạn
    }

    public class BorrowRecord
    {
        public string MaPhieu { get; set; }
        public string MaSinhVien { get; set; }
        public string HoTenSinhVien { get; set; }
        public string ThietBi { get; set; }
        public int SoLuong { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; }
        public DateTime? NgayTraThucTe { get; set; }
        public BorrowStatus TinhTrang { get; set; }
        public string TinhTrangHienThi { get; set; }
    }
}