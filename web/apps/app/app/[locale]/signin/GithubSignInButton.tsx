"use client";
import React from "react";
import GithubLogin from "react-github-login";
import * as process from "process";
import { GithubIcon } from "@icons";

export interface GithubSignInButtonProps {
   onSuccess: (res: { code: string }) => void | Promise<void>;
   onError?: (error: any) => void | Promise<void>;
   className?: string;
}

const GithubSignInButton = ({
   onSuccess,
   onError,
   className,
}: GithubSignInButtonProps) => {
   return (
      <GithubLogin
         redirectUri={`http://localhost:4200`}
         buttonText={
            <div className={`flex mx-auto items-center gap-2`}>
               <div className={`bg-white`}>
                  <GithubIcon fill={"black"} size={24} />
               </div>
               <span>Sign in with Github</span>
            </div>
         }
         className={`bg-white flex items-center justify-center hover:opacity-90 py-2 mt-4 rounded-lg text-small text-black ${className}`}
         id={"github"}
         clientId={process.env.NEXT_PUBLIC_GITHUB_CLIENT_ID}
         onError={onError}
         onSuccess={onSuccess}
      />
   );
};

export default GithubSignInButton;
