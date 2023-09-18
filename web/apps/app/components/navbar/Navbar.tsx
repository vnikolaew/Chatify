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
import { useIsUserLoggedIn } from "@hooks";
import { NotificationsDropdown, UserDropdown } from "@components/navbar";
import { ExitIcon } from "@icons";
import TooltipButton from "@components/common/TooltipButton";
import LogoutIcon from "@components/icons/LogoutIcon";
import SignOutButton from "@components/navbar/SignOutButton";
import AddNewFriendButton from "@components/navbar/AddNewFriendButton";

const NAV_LINKS: (LinkProps & { label: string })[] = [
   {
      href: `/_playgrounds`,
      label: "Playgrounds",
      color: "danger",
   },
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

const MainNavbar = ({ baseImagesUrl }: { baseImagesUrl: string }) => {
   const pathname = usePathname();
   const { isUserLoggedIn } = useIsUserLoggedIn();

   const { data, isLoading, error } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });

   return (
      <Navbar
         classNames={{
            item: ["data-[active=true]:text-primary"],
            base: "py-2",
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
         <NavbarContent justify={"end"} className={`flex gap-1`}>
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
                        // className={`text-foreground group-[data-active=true]:text-primary`}
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

export default MainNavbar;
