using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ThongKe.SampleData;

public static class CreateSampleExcel
{
    public static void CreateDienTichLuaSample()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var sampleData = new[]
        {
            new
            {
                MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Mùa khô", Nam = 2024, Thang = 1,
                DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                DienTichUocTinhThuHoach = 1580.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "02", TenTinhThanh = "Hồ Chí Minh", MuaVu = "Mùa khô", Nam = 2024, Thang = 1,
                DienTichThuHoach = 800.0, DienTichGieoTrong = 850.0, DienTichUocTinhGieoTrong = 900.0,
                DienTichUocTinhThuHoach = 820.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "03", TenTinhThanh = "Đà Nẵng", MuaVu = "Mùa mưa", Nam = 2024, Thang = 6,
                DienTichThuHoach = 300.0, DienTichGieoTrong = 320.0, DienTichUocTinhGieoTrong = 350.0,
                DienTichUocTinhThuHoach = 310.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "04", TenTinhThanh = "Hải Phòng", MuaVu = "Mùa khô", Nam = 2024, Thang = 2,
                DienTichThuHoach = 450.0, DienTichGieoTrong = 480.0, DienTichUocTinhGieoTrong = 500.0,
                DienTichUocTinhThuHoach = 470.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "05", TenTinhThanh = "Cần Thơ", MuaVu = "Mùa mưa", Nam = 2024, Thang = 7,
                DienTichThuHoach = 2200.0, DienTichGieoTrong = 2300.0, DienTichUocTinhGieoTrong = 2400.0,
                DienTichUocTinhThuHoach = 2250.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "06", TenTinhThanh = "An Giang", MuaVu = "Mùa khô", Nam = 2024, Thang = 3,
                DienTichThuHoach = 3500.0, DienTichGieoTrong = 3600.0, DienTichUocTinhGieoTrong = 3700.0,
                DienTichUocTinhThuHoach = 3550.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "07", TenTinhThanh = "Đồng Tháp", MuaVu = "Mùa mưa", Nam = 2024, Thang = 8,
                DienTichThuHoach = 3200.0, DienTichGieoTrong = 3300.0, DienTichUocTinhGieoTrong = 3400.0,
                DienTichUocTinhThuHoach = 3250.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "08", TenTinhThanh = "Kiên Giang", MuaVu = "Mùa khô", Nam = 2024, Thang = 4,
                DienTichThuHoach = 2800.0, DienTichGieoTrong = 2900.0, DienTichUocTinhGieoTrong = 3000.0,
                DienTichUocTinhThuHoach = 2850.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "09", TenTinhThanh = "Long An", MuaVu = "Mùa mưa", Nam = 2024, Thang = 9,
                DienTichThuHoach = 1800.0, DienTichGieoTrong = 1900.0, DienTichUocTinhGieoTrong = 2000.0,
                DienTichUocTinhThuHoach = 1850.0, DonViTinh = "ha"
            },
            new
            {
                MaTinhThanh = "10", TenTinhThanh = "Tiền Giang", MuaVu = "Mùa khô", Nam = 2024, Thang = 5,
                DienTichThuHoach = 1600.0, DienTichGieoTrong = 1700.0, DienTichUocTinhGieoTrong = 1800.0,
                DienTichUocTinhThuHoach = 1650.0, DonViTinh = "ha"
            }
        };

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("DienTichLua");

        // Header
        worksheet.Cells[1, 1].Value = "MaTinhThanh";
        worksheet.Cells[1, 2].Value = "TenTinhThanh";
        worksheet.Cells[1, 3].Value = "MuaVu";
        worksheet.Cells[1, 4].Value = "Nam";
        worksheet.Cells[1, 5].Value = "Thang";
        worksheet.Cells[1, 6].Value = "DienTichThuHoach";
        worksheet.Cells[1, 7].Value = "DienTichGieoTrong";
        worksheet.Cells[1, 8].Value = "DienTichUocTinhGieoTrong";
        worksheet.Cells[1, 9].Value = "DienTichUocTinhThuHoach";
        worksheet.Cells[1, 10].Value = "DonViTinh";

        // Style header
        using (var headerRange = worksheet.Cells[1, 1, 1, 10])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Data
        for (var i = 0; i < sampleData.Length; i++)
        {
            var row = i + 2;
            var data = sampleData[i];

            worksheet.Cells[row, 1].Value = data.MaTinhThanh;
            worksheet.Cells[row, 2].Value = data.TenTinhThanh;
            worksheet.Cells[row, 3].Value = data.MuaVu;
            worksheet.Cells[row, 4].Value = data.Nam;
            worksheet.Cells[row, 5].Value = data.Thang;
            worksheet.Cells[row, 6].Value = data.DienTichThuHoach;
            worksheet.Cells[row, 7].Value = data.DienTichGieoTrong;
            worksheet.Cells[row, 8].Value = data.DienTichUocTinhGieoTrong;
            worksheet.Cells[row, 9].Value = data.DienTichUocTinhThuHoach;
            worksheet.Cells[row, 10].Value = data.DonViTinh;
        }

        // Auto fit columns
        worksheet.Cells.AutoFitColumns();

        // Save file
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "SampleData");
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, "DienTichLua_Sample.xlsx");
        package.SaveAs(new FileInfo(filePath));

        Console.WriteLine($"File Excel mẫu đã được tạo tại: {filePath}");
    }
}