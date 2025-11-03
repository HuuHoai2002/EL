using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThongKe.Migrations
{
    /// <inheritdoc />
    public partial class InitEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BieuMaus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenBieuMau = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    MoTa = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DonViId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    KyBaoCao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PhienBanChiTieuId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ChiTieusJSON = table.Column<string>(type: "CLOB", nullable: false),
                    SoBieuMau = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SoThongTu = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ThongTu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    NgayLapBieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NguoiLapBieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DonViBaoCao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DonViNhanBaoCao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NgayNhanBaoCaoJSON = table.Column<string>(type: "CLOB", nullable: true),
                    CotsJSON = table.Column<string>(type: "CLOB", nullable: false),
                    HangsJSON = table.Column<string>(type: "CLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BieuMaus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiTieuDaNhaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nguon = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Ten = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ChiTieuId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DongDuLieuJSON = table.Column<string>(type: "CLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTieuDaNhaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiTieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenChiTieu = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    Hash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MoTa = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DonViId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhienBanDanhMucId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanTosJSON = table.Column<string>(type: "CLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTieus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoDienTichCayHangNamKhacs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    LoaiCay = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DienTichThuHoach = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichGieoTrong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinhGieoTrong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinhThuHoach = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoDienTichCayHangNamKhacs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoDienTichLuas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MuaVu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DienTichThuHoach = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichGieoTrong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinhGieoTrong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DienTichUocTinhThuHoach = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoDienTichLuas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoSanLuongSanPhamChanNuois",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenTinhThanh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Quy = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TenSanPham = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DonViTinh = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SanLuongThitHoi = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    SanLuongChanNuoiKhac = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoSanLuongSanPhamChanNuois", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongBoXuatKhauNongSans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MaQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TenQuocGiaVungLT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Nam = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Thang = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LoaiNongSan = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DonViKhoiLuong = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KhoiLuong = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DonViGiaTri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    GiaTri = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    IsCongBo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongBoXuatKhauNongSans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenMuc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MaMuc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MaKieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    KyHieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    MoTa = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Stt = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Nguon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ParentMaKieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ParentMaMuc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ChildrenMaKieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ChildrenMaMucs = table.Column<string>(type: "NCLOB", nullable: true),
                    DanhSachsJSON = table.Column<string>(type: "NCLOB", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Updated = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucImport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenPhienBan = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    MaKieuDanhMucs = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RowsType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DanhMucsJSON = table.Column<string>(type: "NCLOB", nullable: true),
                    SoBanGhiThemMoi = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SoBanGhiCapNhat = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SoBanGhiLoi = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ParentMaKieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ParentMaMuc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Nguon = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TrangThai = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Updated = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucImport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucPublish",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenPhienBan = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ThoiGianHetHan = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    MaKieuDanhMucs = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DanhMucsJSON = table.Column<string>(type: "NCLOB", nullable: true),
                    DanhMucMoRongsJSON = table.Column<string>(type: "NCLOB", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Updated = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsDraft = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsPublish = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Published = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucPublish", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DonVis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenDonVi = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    MaDonVi = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    MoTa = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonVis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuLieuBieuMaus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    BieuMauId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NguonDuLieu = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DongDuLieusJSON = table.Column<string>(type: "CLOB", nullable: false),
                    BieuMauJSON = table.Column<string>(type: "CLOB", nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuLieuBieuMaus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuLieuChiTieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Data = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    ChiTieuId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId1 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId2 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId3 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId4 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId5 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId6 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId7 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId8 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId9 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PhanToId10 = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ImportId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuLieuChiTieus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KieuBangThongKes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Ten = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    MoTa = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    ChiTieusJSON = table.Column<string>(type: "CLOB", nullable: false),
                    PhienBanChiTieuId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TableName = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    HashingSchema = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LastTableCreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KieuBangThongKes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KieuDanhMuc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Ten = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Ma = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    MoTa = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Format = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    NhomDanhMuc = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ParentMa = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ChildrenMas = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TruongDuLieusJSON = table.Column<string>(type: "NCLOB", nullable: true),
                    IsOpen = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsClosed = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Updated = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KieuDanhMuc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogAuth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    IsLogin = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsLogout = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    BrowserVersion = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    OSVersion = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DeviceType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IpAddress = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Content = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAuth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MaOrganization = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Phone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Address = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhienBanChiTieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenPhienBan = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PhienBanDanhMucId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ChiTieusJSON = table.Column<string>(type: "CLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienBanChiTieus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FullName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Phone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RoleIds = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    OrganizationId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    SupervisorId = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    RefreshToken = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    IsVerifyRegister = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsInactive = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IsLocked = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Created = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ResetPasswordGuid = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ResetPasswordExpiryTime = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    CountResetPassword = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CountResetPasswordTime = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BieuMaus_CreatedBy",
                table: "BieuMaus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BieuMaus_DonViId",
                table: "BieuMaus",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_BieuMaus_UpdatedBy",
                table: "BieuMaus",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieuDaNhaps_ChiTieuId",
                table: "ChiTieuDaNhaps",
                column: "ChiTieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieuDaNhaps_CreatedBy",
                table: "ChiTieuDaNhaps",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieuDaNhaps_UpdatedBy",
                table: "ChiTieuDaNhaps",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_CreatedBy",
                table: "ChiTieus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_DonViId",
                table: "ChiTieus",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_PhienBanDanhMucId",
                table: "ChiTieus",
                column: "PhienBanDanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_TenChiTieu",
                table: "ChiTieus",
                column: "TenChiTieu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTieus_UpdatedBy",
                table: "ChiTieus",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CongBoDienTichLuas_CreatedBy",
                table: "CongBoDienTichLuas",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CongBoDienTichLuas_UpdatedBy",
                table: "CongBoDienTichLuas",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DonVis_CreatedBy",
                table: "DonVis",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DonVis_MaDonVi",
                table: "DonVis",
                column: "MaDonVi",
                unique: true,
                filter: "\"MaDonVi\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DonVis_UpdatedBy",
                table: "DonVis",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuBieuMaus_BieuMauId",
                table: "DuLieuBieuMaus",
                column: "BieuMauId");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuBieuMaus_CreatedBy",
                table: "DuLieuBieuMaus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuBieuMaus_UpdatedBy",
                table: "DuLieuBieuMaus",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_ChiTieuId",
                table: "DuLieuChiTieus",
                column: "ChiTieuId");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_CreatedBy",
                table: "DuLieuChiTieus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_ImportId",
                table: "DuLieuChiTieus",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId1",
                table: "DuLieuChiTieus",
                column: "PhanToId1");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId10",
                table: "DuLieuChiTieus",
                column: "PhanToId10");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId2",
                table: "DuLieuChiTieus",
                column: "PhanToId2");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId3",
                table: "DuLieuChiTieus",
                column: "PhanToId3");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId4",
                table: "DuLieuChiTieus",
                column: "PhanToId4");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId5",
                table: "DuLieuChiTieus",
                column: "PhanToId5");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId6",
                table: "DuLieuChiTieus",
                column: "PhanToId6");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId7",
                table: "DuLieuChiTieus",
                column: "PhanToId7");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId8",
                table: "DuLieuChiTieus",
                column: "PhanToId8");

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuChiTieus_PhanToId9",
                table: "DuLieuChiTieus",
                column: "PhanToId9");

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_CreatedBy",
                table: "KieuBangThongKes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_HashingSchema",
                table: "KieuBangThongKes",
                column: "HashingSchema",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_PhienBanChiTieuId",
                table: "KieuBangThongKes",
                column: "PhienBanChiTieuId");

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_TableName",
                table: "KieuBangThongKes",
                column: "TableName",
                unique: true,
                filter: "\"TableName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KieuBangThongKes_UpdatedBy",
                table: "KieuBangThongKes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KieuDanhMuc_Ma",
                table: "KieuDanhMuc",
                column: "Ma");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanChiTieus_CreatedBy",
                table: "PhienBanChiTieus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanChiTieus_PhienBanDanhMucId",
                table: "PhienBanChiTieus",
                column: "PhienBanDanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanChiTieus_TrangThai",
                table: "PhienBanChiTieus",
                column: "TrangThai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BieuMaus");

            migrationBuilder.DropTable(
                name: "ChiTieuDaNhaps");

            migrationBuilder.DropTable(
                name: "ChiTieus");

            migrationBuilder.DropTable(
                name: "CongBoDienTichCayHangNamKhacs");

            migrationBuilder.DropTable(
                name: "CongBoDienTichLuas");

            migrationBuilder.DropTable(
                name: "CongBoSanLuongSanPhamChanNuois");

            migrationBuilder.DropTable(
                name: "CongBoXuatKhauNongSans");

            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropTable(
                name: "DanhMucImport");

            migrationBuilder.DropTable(
                name: "DanhMucPublish");

            migrationBuilder.DropTable(
                name: "DonVis");

            migrationBuilder.DropTable(
                name: "DuLieuBieuMaus");

            migrationBuilder.DropTable(
                name: "DuLieuChiTieus");

            migrationBuilder.DropTable(
                name: "KieuBangThongKes");

            migrationBuilder.DropTable(
                name: "KieuDanhMuc");

            migrationBuilder.DropTable(
                name: "LogAuth");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropTable(
                name: "PhienBanChiTieus");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
