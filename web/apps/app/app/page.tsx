import React, { Fragment } from "react";
import Link from "next/link";
import Greeting from "../components/Greeting";
import { cookies } from "next/headers";
import SignOut from "../components/SignOut";

const APPLICATION_COOKIE_NAME = ".AspNetCore.Identity.Application";

function IndexPage() {
   const cookieStore = cookies();
   const isUserLoggedIn = !!cookieStore.get(APPLICATION_COOKIE_NAME);

   return (
      <div
         className={`flex text-xl gap-3 flex-col shadow-gray-300 w-fit px-4 py-2 text-gray-700 rounded-md `}
      >
         <h1 className={`text-3xl`}>Home Page</h1>
         <Greeting />
         {!isUserLoggedIn ? (
            <Fragment>
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
            </Fragment>
         ) : (
            <div>
               <SignOut />
            </div>
         )}
      </div>
   );
}

export default IndexPage;
