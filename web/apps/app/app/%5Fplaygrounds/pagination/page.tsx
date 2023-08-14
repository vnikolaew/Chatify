"use client";
import React from "react";
import { Link, Pagination } from "@nextui-org/react";

const PaginationPage = () => {
   return (
      <div className={`flex min-h-[150vh] m-8 flex-col items-center gap-8`}>
         <div className={`flex-col w-full flex gap-4`}>
            <Pagination initialPage={2} total={20} />
            <Pagination isDisabled initialPage={2} total={20} />
            <Pagination color={"secondary"} initialPage={2} total={20} />
            <Pagination size={"sm"} initialPage={2} total={20} />
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

export default PaginationPage;
