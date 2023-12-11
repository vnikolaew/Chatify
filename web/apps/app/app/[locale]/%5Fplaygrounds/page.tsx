import React, { PropsWithChildren } from "react";
import Link from "next/link";
import Divider from "@components/common/Divider";
import * as fs from "fs";
import * as path from "path";

const GradientLink = ({
   children,
   href,
}: PropsWithChildren & { href: string }) => {
   return (
      <Link
         className={`hover:underline my-2 text-xl text-blue-500`}
         href={href}
      >
         <span
            className={` bg-clip-text text-transparent bg-gradient-to-r from-red-500 to-blue-500`}
         >
            {children}
         </span>
      </Link>
   );
};

const PlaygroundsPage = async (props) => {
   const dir = path.join(
      __dirname,
      "..",
      "..",
      "..",
      "..",
      "..",
      "app",
      "[locale]",
      "%5Fplaygrounds"
   );
   console.log({ dir });

   const components = fs
      .readdirSync(dir, { encoding: "utf8", withFileTypes: true })
      .filter((e) => e.isDirectory())
      .map((e) => e.name);

   return (
      <div className={`m-6 w-full flex flex-col items-center text-center`}>
         <h2 className={`text-3xl`}>
            <b>Playgrounds</b> (for testing purposes)
         </h2>
         <Divider
            className={`shadow-lg w-1/2 h-[1px] rounded-md text-gray-200 mt-3`}
            orientation={"horizontal"}
         />
         <div className={`text-2xl mt-4 w-3/5 grid grid-cols-3 gap-2`}>
            {components.map((c, i) => (
               <GradientLink href={`_playgrounds/${c}`}>
                  {c[0].toUpperCase() + c.slice(1)}
               </GradientLink>
            ))}
         </div>
         <div className={`mt-12`}>
            <Link
               className={`hover:underline ml-auto self-end text-xl text-blue-500`}
               href={`/`}
            >
               Go to Index page
            </Link>
         </div>
      </div>
   );
};

export default PlaygroundsPage;
