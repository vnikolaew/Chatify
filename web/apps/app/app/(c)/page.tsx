import React, { Fragment } from "react";
import Link from "next/link";
import Greeting from "../../components/Greeting";
import { cookies } from "next/headers";
import SignOut from "../../components/SignOut";
import process from "process";

export const revalidate = 0;

async function IndexPage() {
   const cookieStore = cookies();
   const isUserLoggedIn = !!cookieStore.get(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   );

   return (
      <div
         className={`flex min-h-[60vh] text-xl gap-3 flex-col items-center shadow-gray-300 w-full px-4 py-4 rounded-md `}
      >
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
