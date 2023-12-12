"use client";
import React from "react";
import { Code } from "@nextui-org/react";
import Link from "next/link";

const CodePage = () => {
   return (
      <div className={`flex m-8 flex-col gap-8`}>
         <div className={`flex w-1/3 flex-col gap-4 `}>
            <Code color={"primary"}>dotnet new web-api</Code>
            <Code color={"success"} size={"md"}>
               npm install next
            </Code>
            <Code radius={"md"} color={"danger"} size={"lg"}>
               npm install @nextui-org/react
            </Code>
            <Code radius={"lg"} color={"secondary"} size={"lg"}>
               npm install @nextui-org/react
            </Code>
            <Code color={"default"} size={"lg"}>
               npm install @nextui-org/react
            </Code>
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

export default CodePage;
