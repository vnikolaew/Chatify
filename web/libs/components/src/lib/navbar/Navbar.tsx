"use client";
import React, { Fragment } from "react";
import {
   Image,
   Link,
   LinkProps,
   Navbar,
   NavbarBrand,
   NavbarContent,
   NavbarItem,
} from "@nextui-org/react";
import { usePathname } from "next/navigation";
import NextLink from "next/link";
import { useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "@web/hooks";
import SignOutButton from "./SignOutButton";
import AddNewFriendButton from "./AddNewFriendButton";
import { useTranslations } from "next-intl";
import { UserDropdown } from "./UserDropdown";
import { NotificationsDropdown } from "./NotificationsDropdown";

const NAV_LINKS: (LinkProps & { label: string })[] = [
   {
      href: `/signup`,
      color: `foreground`,
      underline: "none",
      label: "Create an account",
   },
   {
      href: `/signin`,
      color: `primary`,
      label: "Sign in",
   },
];

export const MainNavbar = () => {
   const pathname = usePathname();
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const t = useTranslations(`Index`);

   const { data, isLoading, error } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });

   return (
      <Navbar
         classNames={{
            item: ["data-[active=true]:text-primary"],
            base: "py-1",
         }}
         className={`w-full mx-0 !max-w-[3000px]`}
         isBordered
      >
         <NavbarBrand>
            <Link href={`/`} as={NextLink} className={`text-3xl`}>
               <Image
                  radius={"md"}
                  width={36}
                  height={36}
                  src={`favicon.ico`}
               />
               <h2 className={`ml-2 text-foreground`}>Chatify</h2>
            </Link>
         </NavbarBrand>
         <NavbarContent justify={"end"} className={`flex gap-3`}>
            {data?.claims && Object.keys(data?.claims).length ? (
               <Fragment>
                  <NavbarItem className={`mr-4`}>
                     <UserDropdown />
                  </NavbarItem>
                  <NavbarItem>
                     <AddNewFriendButton />
                  </NavbarItem>
                  <NavbarItem>
                     <NotificationsDropdown />
                  </NavbarItem>
                  <NavbarItem>
                     <SignOutButton />
                  </NavbarItem>
               </Fragment>
            ) : (
               NAV_LINKS.map(({ href, color, label, ...rest }, i) => (
                  <NavbarItem isActive={pathname === href} key={i}>
                     <Link
                        size={"lg"}
                        color={(color as any) ?? "foreground"}
                        underline={"hover"}
                        href={href}
                        {...rest}
                     >
                        {label}
                     </Link>
                  </NavbarItem>
               ))
            )}
         </NavbarContent>
      </Navbar>
   );
};
