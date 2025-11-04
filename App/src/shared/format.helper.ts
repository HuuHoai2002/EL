import { isEmpty } from "lodash";

export const formatMoney = (money: number) => {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 9,
  }).format(money);
};

export const formatVietnamNumber = (number: number) => {
  if (number === null || number === undefined) return "";
  if (number === 0) return "0";
  if (isEmpty(number.toString()) || isNaN(number)) return "";
  return new Intl.NumberFormat("vi-VN", {
    style: "decimal",
    maximumFractionDigits: 9,
  }).format(number);
};
