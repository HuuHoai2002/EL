export const toSet = (data: any[]) => {
  return [...new Set(data)];
};

export const findItem = (
  data: Record<string, any>[],
  key: string,
  value: string
) => data.find((s) => s[key] === value);

export const getLastItem = <T>(arr: T[]): T | null => {
  if (!Array.isArray(arr) || arr.length === 0) {
    return null;
  }
  return arr[arr.length - 1];
};

export const insertArrayAtIndex = (
  array: any[],
  index: number,
  insertArray: any[],
  overwrite = false
): any[] => {
  if (index < 0 || index > array.length) {
    throw new Error("Index out of bounds");
  }
  const before = array.slice(0, index);
  const newIndex = overwrite ? index + 1 : index;
  const after = array.slice(newIndex);
  return [...before, ...insertArray, ...after];
};

export const overwriteArrayAtIndex = (
  array: any[],
  index: number,
  insertArray: any[]
): any[] => {
  if (index < 0 || index > array.length) {
    throw new Error("Index out of bounds");
  }
  return [...array.slice(0, index), ...insertArray, ...array.slice(index + 1)];
};

export const toUniqueObjectArray = (arr: Record<string, any>[]) => {
  try {
    const parsed = arr.map((r) => JSON.stringify(r));
    return toSet(parsed).map((r) => JSON.parse(r));
  } catch (error) {
    console.error(error);
    return arr;
  }
};
