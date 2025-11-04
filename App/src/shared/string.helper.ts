import { isEmpty, isObject } from "lodash";
import { vnNumberRegex } from "./regex.helper";

export const removeVietnameseDiacritics = (str: string) => {
  return str
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
};

export const parseStringToNumber = (value: string): number | null => {
  if (value === "") return 0;
  if (!value || typeof value !== "string") return null;

  const trimmed = value.replace(/\s+/g, "");
  if (trimmed === "") return 0;

  if (trimmed.includes(",")) {
    const vietnameseFormat = trimmed.replace(/\./g, "").replace(",", ".");
    const num = parseFloat(vietnameseFormat);
    return isNaN(num) ? null : num;
  }
  const dotCount = (trimmed.match(/\./g) || []).length;
  if (dotCount >= 2 && vnNumberRegex.test(trimmed)) {
    const vietnameseNumber = trimmed.replace(/\./g, "");
    const num = parseFloat(vietnameseNumber);
    return isNaN(num) ? null : num;
  }
  const standardNumber = trimmed.replace(/,/g, "");
  const num = parseFloat(standardNumber);
  return isNaN(num) ? null : num;
};

export const numberToWordsVN = (
  value: number | string,
  allowNegative: boolean = true
): string => {
  if (!value && value !== 0) return "";
  const numValue = parseInt(value.toString(), 10);
  let num = Math.abs(numValue);
  const isNegative = allowNegative && numValue < 0;
  const units: string[] = [
    "không",
    "một",
    "hai",
    "ba",
    "bốn",
    "năm",
    "sáu",
    "bảy",
    "tám",
    "chín",
  ];
  const tens: string[] = [
    "",
    "mười",
    "hai mươi",
    "ba mươi",
    "bốn mươi",
    "năm mươi",
    "sáu mươi",
    "bảy mươi",
    "tám mươi",
    "chín mươi",
  ];
  const hundreds = "trăm";
  const thousands: string[] = ["nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ"];

  const convertLessThanThousand = (num: number): string => {
    let result = "";

    if (num >= 100) {
      result += units[Math.floor(num / 100)] + " " + hundreds;
      num %= 100;
      if (num > 0) result += " ";
    }

    if (num >= 10) {
      if (num === 10) {
        result += tens[1];
        return result;
      }
      result += tens[Math.floor(num / 10)];
      num %= 10;
      if (num > 0) result += " ";
    }

    if (num > 0) {
      if (num === 5 && result.includes("mươi")) {
        result += "lăm";
      } else if (
        num === 1 &&
        result.includes("mươi") &&
        !result.includes("mười")
      ) {
        result += "mốt";
      } else {
        result += units[num];
      }
    }

    return result;
  };

  if (num === 0) return units[0];

  let result = "";
  let groupIndex = 0;

  while (num > 0) {
    const threeDigits = num % 1000;
    if (threeDigits > 0) {
      let groupText = convertLessThanThousand(threeDigits);
      if (groupIndex > 0) {
        groupText += " " + thousands[groupIndex];
      }
      result = groupText + (result ? " " + result : "");
    }
    num = Math.floor(num / 1000);
    groupIndex++;
  }
  if (isNegative) {
    result = "âm " + result;
  }
  return result.charAt(0).toUpperCase() + result.slice(1);
};

export const cleanedExcelStrings = (data: string, splitPattern = /[;\n]/) => {
  return data
    ?.trim()
    ?.split(splitPattern)
    ?.map((s) => s.trim())
    ?.map((s) => s?.replace(/^\t+|\t+$|\t(?=\t)/g, ""));
};

export const cleanedExcelStringsToNumber = (
  data: string,
  splitPattern = /[;\n]/
) => {
  return data
    ?.split(splitPattern)
    ?.map((stat) => (isEmpty(stat) ? "0" : stat))
    ?.map((s) => s.trim())
    ?.map((s) => s?.replace(/^\t+|\t+$|\t(?=\t)/g, ""));
};

export const encodedBase64 = (data: Record<string, any> | string | number) => {
  try {
    return btoa(isObject(data) ? JSON.stringify(data) : data.toString());
  } catch (error) {
    console.error(error);
    return null;
  }
};

export const decodedBase64 = (
  base64String: string
): Record<string, any> | null => {
  try {
    return JSON.parse(atob(base64String));
  } catch (error: any) {
    return null;
  }
};

export const normalizeString = (s: string) => {
  // Chỉ cho phép 1 dấu cách liên tiếp
  s = s.replace(/\s+/g, " ").trim();
  return s.normalize("NFC");
};

export const normalizeDeepString = (s: string) => {
  s = s.replace(/\s/g, "");
  // bỏ các ký tự đặc biệt
  s = s.replace(/[^a-zA-Z0-9\s]/g, "");

  return s.normalize("NFC").toLowerCase();
};

export const normalizeAndLowercaseString = (s: string) => {
  // Chỉ cho phép 1 dấu cách liên tiếp
  s = s.replace(/\s+/g, " ").trim();
  return s.normalize("NFC").toLowerCase();
};
