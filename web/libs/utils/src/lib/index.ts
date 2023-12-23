import { unified } from "unified";
import remarkParse from "remark-parse";
import remarkRehype from "remark-rehype";
import rehypeStringify from "rehype-stringify";
import process from "process";

export function __IS_DEV__() {
   return process.env.NODE_ENV === "development";
}

export function hexToDecimal(hexString: string) {
   // Remove any leading "0x" if present
   if (hexString.startsWith("0x")) {
      hexString = hexString.substring(2);
   }

   // Convert the hexadecimal string to a decimal number
   return parseInt(hexString, 16);
}

export function downloadImage(url: string, filename: string) {
   fetch(url)
      .then((response) => response.blob())
      .then((blob) => {
         const blobUrl = URL.createObjectURL(blob);
         const link = document.createElement("a");
         link.href = blobUrl;
         link.download = filename || "image.jpg";
         link.style.display = "none";

         document.body.appendChild(link);
         link.click();
         document.body.removeChild(link);

         URL.revokeObjectURL(blobUrl);
      })
      .catch((error) => {
         console.error(`Error downloading image: ${error}`);
      });
}

export const normalizeFileName = (fileName: string) => {
   const parts = fileName.split(".");
   return `${parts
      .slice(0, parts.length - 1)
      .join("")
      .substring(0, 15)}... .${parts.at(-1)}`;
};

export const randomShuffleArray = <T>(array: T[]) => {
   return [...array].sort((_) => Math.random() - 0.5);
};

export const markdownProcessor = unified()
   // @ts-ignore
   .use(remarkParse)
   // @ts-ignore
   .use(remarkRehype)
   .use(rehypeStringify);
