"use client";
import { useEffect, useRef } from "react";

export function useInterval(
   callback: () => void,
   intervalMs: number,
   deps: any[]
) {
   const intervalRef = useRef<number>(null!);

   useEffect(() => {
      // @ts-ignore
      intervalRef.current = setInterval(callback, intervalMs);
      return () => clearInterval(intervalRef.current);
   }, deps);

   return intervalRef;
}
