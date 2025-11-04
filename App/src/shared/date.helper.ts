import moment from "moment";

moment.locale("vi");

export const getDateString = (date: string | Date, hint?: string) => {
  if (!date) return hint ?? "Chưa cập nhật";
  return moment(date).format("DD/MM/YYYY");
};

export const fromNow = (date: string | Date) => {
  return moment(date).locale("vi").fromNow();
};

export const getDateTimeString = (date: string | Date, hint?: string) => {
  if (!date) return hint ?? "Chưa cập nhật";
  return moment(date).format("DD/MM/YYYY HH:mm");
};
