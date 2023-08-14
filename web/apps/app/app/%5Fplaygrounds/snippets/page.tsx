"use client";
import React, { Fragment } from "react";
import Link from "next/link";
import { Snippet } from "@nextui-org/react";

const SnippetsPage = () => {
   return (
      <div className={`flex m-8 w-1/3 flex-col gap-8`}>
         <div className={`flex flex-col items-center gap-4`}>
            {["sm", "md", "lg"].map((size, i) => (
               <Fragment key={i}>
                  <Snippet
                     color={"primary"}
                     tooltipProps={{
                        "aria-label": "Copy this snippet",
                        "color": "foreground",
                        "content": "Copy this snippet",
                        "title": "Copy this snippet",
                        "placement": "right",
                     }}
                     variant={"flat"}
                     size={size as any}
                     className={`overflow-hidden`}
                  >
                     dotnet install -g ef core
                  </Snippet>
                  <Snippet
                     variant={"shadow"}
                     color={"danger"}
                     size={size as any}
                     className={`overflow-hidden`}
                  >
                     dotnet install -g ef core
                  </Snippet>
               </Fragment>
            ))}
         </div>
         <Link
            className={`hover:underline ml-auto self-end text-xl text-blue-500`}
            href={`/_playgrounds`}
         >
            Go Back
         </Link>
      </div>
   );
};

export default SnippetsPage;
