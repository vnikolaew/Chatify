import React, { Fragment } from "react";
import Link from "next/link";
import Greeting from "../components/Greeting";
import { cookies } from "next/headers";
import SignOut from "../components/SignOut";
import * as process from "process";

const APPLICATION_COOKIE_NAME = ".AspNetCore.Identity.Application";

function IndexPage() {
   const cookieStore = cookies();
   const isUserLoggedIn = !!cookieStore.get(APPLICATION_COOKIE_NAME);

   console.log(isUserLoggedIn && "User has cookie");

   console.log(process.env);
   return (
      <div
         className={`flex text-xl gap-3 flex-col shadow-gray-300 w-fit px-4 py-2 text-gray-700 rounded-md `}
      >
         <h1 className={`text-3xl`}>Home Page</h1>
         <Greeting
            imagesBaseUrl={`${process.env.NEXT_PUBLIC_BACKEND_API_URL.slice(
               0,
               -4
            )}/static`}
         />
         {!isUserLoggedIn ? (
            <Fragment>
               <h2 className={`font-bold`}>You are currently not logged in.</h2>
               <div className={`flex items-center gap-8`}>
                  <Link
                     className={`hover:underline text-blue-500`}
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
            <div className={`self-end`}>
               <SignOut />
            </div>
         )}
      </div>
   );
}

export default IndexPage;
