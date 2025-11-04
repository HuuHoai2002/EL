import { create } from "zustand";

type GlobalState = {};

export const useGlobalStore = create<GlobalState>((set, get) => ({}));
