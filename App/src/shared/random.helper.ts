export const randomNumber = (min = 99999, max = 999999999): number => {
  if (min >= max) {
    throw new Error("Min must be less than Max");
  }
  return Math.floor(Math.random() * (max - min + 1)) + min;
};
