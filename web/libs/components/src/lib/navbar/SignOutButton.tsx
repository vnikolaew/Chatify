"use client";
import React from "react";
import { useSignOutMutation } from "@web/api";
import { useTranslations } from "next-intl";
import { Spinner } from "@nextui-org/react";
import { TooltipButton } from "../common";
import LogoutIcon from "../icons/LogoutIcon";

export interface SignOutButtonProps {
}

const SignOutButton = ({}: SignOutButtonProps) => {
   const { mutateAsync: signOut, isLoading } = useSignOutMutation();
   const t = useTranslations("MainNavbar.Popups");

   const handleSignOut = async () => {
      await signOut({});
      window.location.reload();
   };

   if (isLoading) {
      return <Spinner size={`sm`} color={`danger`} />;
   }

   return (
      <TooltipButton
         onClick={handleSignOut}
         className={`text-xs px-3 py-1`}
         size={`sm`}
         shadow={`sm`}
         classNames={{
            base: `text-xs `,
            content: `!text-[10px] h-5`,
         }}
         chipProps={{
            className: `hover:bg-danger-500 w-8 h-8 p-0`,
            classNames: {
               content: `w-8 text-small h-8 p-0 flex items-center justify-center`,
            },
         }}
         icon={
            <LogoutIcon
               className={`fill-danger-500 group-hover:fill-white`}
               size={16}
            />
         }
         content={t(`SignOut`)}
      />
   );
};

export default SignOutButton;
