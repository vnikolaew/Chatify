"use client";
import React, { Dispatch, MutableRefObject, SetStateAction, useEffect, useRef, useState } from "react";

export function useHover<TRef extends HTMLElement>(): [
   MutableRefObject<TRef>,
   boolean,
   Dispatch<SetStateAction<boolean>>
] {
   const [hovering, setHovering] = useState(false);
   const ref = useRef<TRef>(null!);

   useEffect(() => {
      const node = ref.current;

      if (!node) return;

      const handleMouseEnter = () => {
         setHovering(true);
      };

      const handleMouseLeave = () => {
         setHovering(false);
      };

      node.addEventListener("mouseenter", handleMouseEnter, {passive: true});
      node.addEventListener("mouseleave", handleMouseLeave, { passive: true});

      return () => {
         node.removeEventListener("mouseenter", handleMouseEnter);
         node.removeEventListener("mouseleave", handleMouseLeave);
      };
   }, []);

   return [ref, hovering, setHovering];
}
