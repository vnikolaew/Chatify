"use client";
import TooltipButton from "@components/common/TooltipButton";
import React from "react";
import { useSignOutMutation } from "@web/api";
import LogoutIcon from "@components/icons/LogoutIcon";
import { useTranslations } from "next-intl";

export interface SignOutButtonProps {}

const SignOutButton = ({}: SignOutButtonProps) => {
   const { mutateAsync: signOut } = useSignOutMutation();
   const t = useTranslations('MainNavbar.Popups');

   const handleSignOut = async () => {
      await signOut({});
      window.location.reload();
   };

   return (
      <TooltipButton
         onClick={handleSignOut}
         className={`text-xs px-3 py-1`}
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
