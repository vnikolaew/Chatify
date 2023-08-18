"use client";
import React, { Fragment } from "react";
import Link from "next/link";
import Greeting from "../../components/Greeting";
import SignOut from "../../components/SignOut";
import { Avatar } from "@nextui-org/react";
import { useSearchParams } from "next/navigation";
import { useGetChatGroupDetailsQuery } from "@web/api";
import useCookie from "react-use-cookie";
import process from "process";

export const revalidate = 0;

function IndexPage(props) {
   const params = useSearchParams();
   const chatGroupId = params.get("c");
   const { data, error, isLoading } = useGetChatGroupDetailsQuery(chatGroupId);

   console.log(chatGroupId);
   const isUserLoggedIn = !!useCookie(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   )[0];

   return (
      <div
         className={`flex min-h-[60vh] text-xl gap-3 flex-col items-center shadow-gray-300 w-full rounded-md `}
      >
         <div
            className={`flex border-b-1 border-b-default-200 w-full items-center p-2 gap-2`}
         >
            {/*<Avatar />*/}
            Chat Group: {chatGroupId}
         </div>
         <h1 className={`text-3xl my-4`}>Home Page</h1>
         {isUserLoggedIn && <Greeting />}
         {!isUserLoggedIn ? (
            <Fragment>
               <h2 className={`font-bold`}>You are currently not logged in.</h2>
               <div className={`flex items-center gap-8`}>
                  <Link
                     className={`hover:underline text-blue-600`}
                     href={`/signup`}
                  >
                     {" "}
                     Sign Up
                  </Link>
                  <Link
                     className={`hover:underline text-blue-600`}
                     href={`/signin`}
                  >
                     Sign In
                  </Link>
               </div>
            </Fragment>
         ) : (
            <div className={`self-center mt-4`}>
               <SignOut />
            </div>
         )}
      </div>
   );
}

export default IndexPage;
