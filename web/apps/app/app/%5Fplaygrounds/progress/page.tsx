"use client";
import React from "react";
import { CircularProgress, Progress } from "@nextui-org/react";
import Link from "next/link";

const ProgressPage = () => {
   return (
      <div className={`flex m-8 w-max flex-col gap-8`}>
         <div className={`grid gap-4 grid-cols-4`}>
            <CircularProgress color={"primary"} />
            <CircularProgress label={"50%"} value={50} color={"warning"} />
            <CircularProgress
               formatOptions={{ unit: "kilometer", style: "unit" }}
               label={"Speed"}
               value={50}
               size={"lg"}
               showValueLabel
               color={"success"}
            />
         </div>
         <Progress isStriped size={"sm"} color={"default"} value={60} />
         <Progress size={"lg"} color={"primary"} value={20} />
         <Progress color={"danger"} value={80} />
         <Progress color={"warning"} value={30} />
         <Progress isStriped color={"warning"} value={70} />
         <Progress
            label={"Monthly progress"}
            size={"md"}
            showValueLabel
            color={"warning"}
            value={70}
         />
         <Link
            className={`hover:underline ml-auto self-end text-xl text-blue-500`}
            href={`/_playgrounds`}
         >
            Go Back
         </Link>
      </div>
   );
};

export default ProgressPage;
