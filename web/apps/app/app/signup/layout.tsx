"use client";
import React, { PropsWithChildren } from "react";
import { GoogleOAuthProvider } from "@react-oauth/google";

const SignUpLayout = ({ children }: PropsWithChildren) => {
   return (
      <GoogleOAuthProvider
         clientId={`353777828224-ob34pac9tb6362bk8lrnfa4tj61ue8tv.apps.googleusercontent.com`}
      >
         {children}
      </GoogleOAuthProvider>
   );
};

export default SignUpLayout;
