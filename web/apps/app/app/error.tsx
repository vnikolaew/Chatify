"use client";
import React, { useEffect } from "react";
import { Button } from "@nextui-org/react";

const ErrorPage = ({
   error,
   reset,
}: {
   error: Error & { digest?: string };
   reset: () => void;
}) => {
   useEffect(() => {
      console.error(error);
   }, [error]);

   return (
      <div className={`w-full flex flex-col gap-4 items-center`}>
         <h2>Oops, Something went wrong!</h2>
         <Button onClick={(_) => reset()} color={"primary"} variant={"solid"}>
            Try again
         </Button>
      </div>
   );
};

export default ErrorPage;
