using System;
using System.Collections.Generic;
using System.Linq;

namespace ThongKe.Shared.Utils
{
    public static class StatisticsUtils
    {
        public static DescriptiveStatistics CalculateDescriptiveStatistics(IEnumerable<double> values)
        {
            var data = values.Where(x => !double.IsNaN(x) && !double.IsInfinity(x)).ToList();
            
            if (!data.Any())
            {
                return new DescriptiveStatistics
                {
                    Count = 0,
                    Mean = 0,
                    Median = 0,
                    Variance = 0,
                    StandardDeviation = 0,
                    Skewness = 0,
                    Kurtosis = 0,
                    Min = 0,
                    Max = 0,
                    Sum = 0
                };
            }

            var count = data.Count;
            var sum = data.Sum();
            var mean = sum / count;
            
            // Median
            var sortedData = data.OrderBy(x => x).ToList();
            var median = count % 2 == 0 
                ? (sortedData[count / 2 - 1] + sortedData[count / 2]) / 2.0
                : sortedData[count / 2];

            // Variance (population variance: n)
            var variance = count > 0 
                ? data.Sum(x => Math.Pow(x - mean, 2)) / count
                : 0;
            
            var standardDeviation = Math.Sqrt(variance);

            // Skewness
            var skewness = CalculateSkewness(data, mean, standardDeviation);
            
            // Kurtosis
            var kurtosis = CalculateKurtosis(data, mean, standardDeviation);

            // Quartiles và IQR
            var q1 = CalculateQuartile(sortedData, 0.25);
            var q3 = CalculateQuartile(sortedData, 0.75);
            var iqr = q3 - q1;

            // Coefficient of Variation
            var coefficientOfVariation = mean != 0 ? (standardDeviation / Math.Abs(mean)) * 100 : 0;

            // Phân tích outliers
            var outlierAnalysis = AnalyzeOutliers(data, mean, standardDeviation, q1, q3, iqr);

            // Phân tích phân phối
            var distributionAnalysis = new DistributionAnalysis
            {
                Skewness = skewness,
                Kurtosis = kurtosis,
                CoefficientOfVariation = coefficientOfVariation,
                IsNormalDistribution = Math.Abs(skewness) < 0.5 && Math.Abs(kurtosis - 3) < 0.5,
                NormalityScore = CalculateNormalityScore(skewness, kurtosis)
            };

            return new DescriptiveStatistics
            {
                Count = count,
                Mean = mean,
                Median = median,
                Variance = variance,
                StandardDeviation = standardDeviation,
                Skewness = skewness,
                Kurtosis = kurtosis,
                Min = sortedData.First(),
                Max = sortedData.Last(),
                Sum = sum,
                Q1 = q1,
                Q3 = q3,
                IQR = iqr,
                CoefficientOfVariation = coefficientOfVariation,
                OutlierAnalysis = outlierAnalysis,
                DistributionAnalysis = distributionAnalysis
            };
        }

        private static double CalculateSkewness(List<double> data, double mean, double standardDeviation)
        {
            if (data.Count < 3 || standardDeviation == 0) return 0;

            var n = data.Count;
            var skewnessSum = data.Sum(x => Math.Pow((x - mean) / standardDeviation, 3));
            
            // Population skewness formula
            return skewnessSum / n;
        }

        private static double CalculateKurtosis(List<double> data, double mean, double standardDeviation)
        {
            if (data.Count < 4 || standardDeviation == 0) return 0;

            var n = data.Count;
            var kurtosisSum = data.Sum(x => Math.Pow((x - mean) / standardDeviation, 4));
            
            // Population kurtosis formula
            return kurtosisSum / n;
        }

        private static double CalculateQuartile(List<double> sortedData, double quartile)
        {
            var n = sortedData.Count;
            var index = quartile * (n - 1);
            
            if (index == Math.Floor(index))
            {
                return sortedData[(int)index];
            }
            else
            {
                var lowerIndex = (int)Math.Floor(index);
                var upperIndex = lowerIndex + 1;
                var weight = index - lowerIndex;
                
                return sortedData[lowerIndex] * (1 - weight) + sortedData[upperIndex] * weight;
            }
        }

        private static OutlierAnalysis AnalyzeOutliers(List<double> data, double mean, double standardDeviation, double q1, double q3, double iqr)
        {
            var outlierAnalysis = new OutlierAnalysis();
            
            // IQR Method
            var lowerBound = q1 - 1.5 * iqr;
            var upperBound = q3 + 1.5 * iqr;
            outlierAnalysis.IQROutliers = data.Where(x => x < lowerBound || x > upperBound).Distinct().ToList();

            // Z-Score Method (|z| > 2.5)
            if (standardDeviation > 0)
            {
                outlierAnalysis.ZScoreOutliers = data.Where(x => Math.Abs((x - mean) / standardDeviation) > 2.5).Distinct().ToList();
            }

            // Modified Z-Score Method (using median)
            var median = CalculateQuartile(data.OrderBy(x => x).ToList(), 0.5);
            var mad = CalculateMAD(data, median);
            if (mad > 0)
            {
                outlierAnalysis.ModifiedZScoreOutliers = data.Where(x => Math.Abs(0.6745 * (x - median) / mad) > 3.5).Distinct().ToList();
            }

            // Tổng hợp outliers (lấy union của tất cả methods)
            var allOutliers = outlierAnalysis.IQROutliers
                .Union(outlierAnalysis.ZScoreOutliers)
                .Union(outlierAnalysis.ModifiedZScoreOutliers)
                .Distinct()
                .ToList();

            outlierAnalysis.TotalOutliers = allOutliers.Count;
            outlierAnalysis.OutlierPercentage = data.Count > 0 ? (double)allOutliers.Count / data.Count * 100 : 0;

            return outlierAnalysis;
        }

        private static double CalculateMAD(List<double> data, double median)
        {
            var deviations = data.Select(x => Math.Abs(x - median)).ToList();
            return CalculateQuartile(deviations.OrderBy(x => x).ToList(), 0.5);
        }

        private static double CalculateNormalityScore(double skewness, double kurtosis)
        {
            // Tính điểm từ 0-100, với 100 là phân phối chuẩn hoàn hảo
            var skewnessScore = Math.Max(0, 100 - Math.Abs(skewness) * 50);
            var kurtosisScore = Math.Max(0, 100 - Math.Abs(kurtosis - 3) * 25);
            
            return (skewnessScore + kurtosisScore) / 2;
        }
    }

    public class DescriptiveStatistics
    {
        public int Count { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Variance { get; set; }
        public double StandardDeviation { get; set; }
        public double Skewness { get; set; }
        public double Kurtosis { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Sum { get; set; }
        
        // Thêm phân tích phân phối và outliers
        public double Q1 { get; set; }
        public double Q3 { get; set; }
        public double IQR { get; set; }
        public double CoefficientOfVariation { get; set; }
        public OutlierAnalysis OutlierAnalysis { get; set; } = new OutlierAnalysis();
        public DistributionAnalysis DistributionAnalysis { get; set; } = new DistributionAnalysis();
        
        // Thêm giải thích cho người dùng
        public string SkewnessInterpretation => GetSkewnessInterpretation();
        public string KurtosisInterpretation => GetKurtosisInterpretation();

        private string GetSkewnessInterpretation()
        {
            if (Math.Abs(Skewness) < 0.5) return "Phân phối gần như đối xứng";
            if (Skewness > 0.5) return "Phân phối lệch phải (đuôi dài bên phải)";
            return "Phân phối lệch trái (đuôi dài bên trái)";
        }

        private string GetKurtosisInterpretation()
        {
            if (Math.Abs(Kurtosis - 3) < 0.5) return "Độ nhọn gần như phân phối chuẩn";
            if (Kurtosis > 3) return "Phân phối nhọn hơn phân phối chuẩn";
            return "Phân phối tù hơn phân phối chuẩn";
        }
    }

    public class OutlierAnalysis
    {
        public List<double> IQROutliers { get; set; } = new List<double>();
        public List<double> ZScoreOutliers { get; set; } = new List<double>();
        public List<double> ModifiedZScoreOutliers { get; set; } = new List<double>();
        public int TotalOutliers { get; set; }
        public double OutlierPercentage { get; set; }
        public string OutlierSeverity => GetOutlierSeverity();
        public string OutlierInterpretation => GetOutlierInterpretation();

        private string GetOutlierSeverity()
        {
            if (OutlierPercentage == 0) return "Không có bất thường";
            if (OutlierPercentage < 5) return "Bất thường thấp";
            if (OutlierPercentage < 10) return "Bất thường vừa phải";
            return "Bất thường cao";
        }

        private string GetOutlierInterpretation()
        {
            if (OutlierPercentage == 0) return "Dữ liệu rất đồng nhất, không có giá trị bất thường";
            if (OutlierPercentage < 5) return "Dữ liệu tương đối ổn định với một số giá trị ngoại lệ nhỏ";
            if (OutlierPercentage < 10) return "Có một số giá trị bất thường cần xem xét";
            return "Nhiều giá trị bất thường, cần kiểm tra chất lượng dữ liệu";
        }
    }

    public class DistributionAnalysis
    {
        public string DistributionType => GetDistributionType();
        public string DataQuality => GetDataQuality();
        public string Recommendation => GetRecommendation();
        public bool IsNormalDistribution { get; set; }
        public double NormalityScore { get; set; }

        // Dựa trên Skewness và Kurtosis để đánh giá
        public double Skewness { get; set; }
        public double Kurtosis { get; set; }
        public double CoefficientOfVariation { get; set; }

        private string GetDistributionType()
        {
            if (Math.Abs(Skewness) < 0.5 && Math.Abs(Kurtosis - 3) < 0.5)
                return "Phân phối chuẩn";
            if (Skewness > 1) return "Phân phối lệch phải mạnh";
            if (Skewness < -1) return "Phân phối lệch trái mạnh";
            if (Skewness > 0.5) return "Phân phối lệch phải";
            if (Skewness < -0.5) return "Phân phối lệch trái";
            if (Kurtosis > 4) return "Phân phối nhọn";
            if (Kurtosis < 2) return "Phân phối tù";
            return "Phân phối gần chuẩn";
        }

        private string GetDataQuality()
        {
            var qualityScore = 0;
            
            // Đánh giá dựa trên CV
            if (CoefficientOfVariation < 15) qualityScore += 3;
            else if (CoefficientOfVariation < 25) qualityScore += 2;
            else if (CoefficientOfVariation < 35) qualityScore += 1;

            // Đánh giá dựa trên tính đối xứng
            if (Math.Abs(Skewness) < 0.5) qualityScore += 2;
            else if (Math.Abs(Skewness) < 1) qualityScore += 1;

            // Đánh giá dựa trên độ nhọn
            if (Math.Abs(Kurtosis - 3) < 0.5) qualityScore += 2;
            else if (Math.Abs(Kurtosis - 3) < 1) qualityScore += 1;

            return qualityScore switch
            {
                >= 6 => "Chất lượng dữ liệu tốt",
                >= 4 => "Chất lượng dữ liệu khá",
                >= 2 => "Chất lượng dữ liệu trung bình",
                _ => "Chất lượng dữ liệu kém"
            };
        }

        private string GetRecommendation()
        {
            var recommendations = new List<string>();

            if (CoefficientOfVariation > 30)
                recommendations.Add("Dữ liệu biến động cao, cần kiểm tra tính nhất quán");

            if (Math.Abs(Skewness) > 1)
                recommendations.Add("Dữ liệu lệch mạnh, cần xem xét phương pháp chuẩn hóa");

            if (Kurtosis > 4)
                recommendations.Add("Có nhiều giá trị cực đoan, cần kiểm tra outliers");

            if (Kurtosis < 2)
                recommendations.Add("Dữ liệu phân tán đều, phù hợp cho phân tích xu hướng");

            if (!recommendations.Any())
                recommendations.Add("Dữ liệu có chất lượng tốt, phù hợp cho các phân tích thống kê");

            return string.Join("; ", recommendations);
        }
    }
}