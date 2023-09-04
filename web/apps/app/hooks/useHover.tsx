import React, { MutableRefObject, useEffect, useRef, useState } from "react";

export function useHover<TRef extends HTMLElement>(): [
   MutableRefObject<TRef>,
   boolean
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

      node.addEventListener("mouseenter", handleMouseEnter);
      node.addEventListener("mouseleave", handleMouseLeave);

      return () => {
         node.removeEventListener("mouseenter", handleMouseEnter);
         node.removeEventListener("mouseleave", handleMouseLeave);
      };
   }, []);

   return [ref, hovering];
}
