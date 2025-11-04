import { isEmpty, isObject } from "lodash";

export const removeEmptyObject = (obj: Record<string, any>) => {
  if (isEmpty(obj)) return;
  Object.keys(obj).forEach((key) => {
    if (obj[key] instanceof Date) {
      obj[key] = obj[key].toISOString();
      return;
    }
    if (isObject(obj[key])) {
      if (Array.isArray(obj[key])) return;
      if (isEmpty(obj[key])) {
        delete obj[key];
      } else {
        removeEmptyObject(obj[key]);
        if (isEmpty(obj[key])) {
          delete obj[key];
        }
      }
    } else if (obj[key] === null || obj[key] === undefined || obj[key] === "") {
      delete obj[key];
    }
  });
  return obj;
};
