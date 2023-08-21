"use client";
import React, { useState } from "react";
import {
   Avatar,
   Badge,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownSection,
   DropdownTrigger,
   Image,
   Link,
   Navbar,
   NavbarBrand,
   NavbarContent,
   NavbarItem,
   Switch,
} from "@nextui-org/react";
import useCookie from "react-use-cookie";
import { usePathname } from "next/navigation";
import NextLink from "next/link";
import {
   useGetMyClaimsQuery,
   useGetUserDetailsQuery,
   useSignOutMutation,
} from "@web/api";
import { isValidURL } from "./Greeting";
import ExitIcon from "./icons/ExitIcon";
import ProfileIcon from "./icons/ProfileIcon";
import { useTheme } from "next-themes";
import { UserStatus } from "../../../libs/api/openapi";

const NAV_LINKS = [
   {
      href: `/_playgrounds`,
      label: "Playgrounds",
      color: "danger",
   },
   {
      href: `/signup`,
      label: "Sign Up",
   },
   {
      href: `/signin`,
      label: "Sign In",
   },
];

const MainNavbar = ({ baseImagesUrl }: { baseImagesUrl: string }) => {
   const pathname = usePathname();
   const userHasCookie = !!useCookie(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME,
      null
   )[0];
   const {
      data: claims,
      isLoading,
      error,
   } = useGetMyClaimsQuery({
      select: (data) => data.claims,
      enabled: userHasCookie,
   });
   const { data: userDetails } = useGetUserDetailsQuery(
      claims?.nameidentifier,
      {
         enabled: !!claims?.nameidentifier,
      }
   );

   const { theme, setTheme } = useTheme();
   const [isDropdownMenuOpen, setIsDropdownMenuOpen] = useState(false);
   const { mutateAsync: signOut } = useSignOutMutation();

   console.log(userDetails);

   const handleSignOut = async () => {
      await signOut(null!);
      window.location.reload();
   };

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
         <NavbarContent justify={"end"} className={`flex gap-4`}>
            {claims && Object.keys(claims).length ? (
               <NavbarItem>
                  <Dropdown
                     showArrow
                     offset={10}
                     // onOpenChange={setIsDropdownMenuOpen}
                     // isOpen={isDropdownMenuOpen}
                  >
                     <DropdownTrigger>
                        <div
                           className={`flex transition-opacity duration-300 cursor-pointer hover:opacity-90 items-center gap-4`}
                        >
                           <Badge // disableOutline
                              content={""}
                              shape={"circle"}
                              color={
                                 userDetails?.status === UserStatus.ONLINE
                                    ? "success"
                                    : userDetails?.status === UserStatus.AWAY
                                    ? "warning"
                                    : "default"
                              }
                              size={"sm"}
                              placement={"bottom-right"}
                           >
                              <Avatar
                                 src={
                                    isValidURL(claims.picture)
                                       ? claims.picture
                                       : `${baseImagesUrl}/${claims.picture}`
                                 }
                                 size={"md"}
                                 alt={"profile-picture"}
                                 radius={"full"}
                                 color={"default"}
                              />
                           </Badge>
                           <span
                              className={`text-small light:text-red dark:text-foreground-500`}
                           >
                              {claims.name}
                           </span>
                        </div>
                     </DropdownTrigger>
                     <DropdownMenu
                        className={`gap-2 min-w-[200px]`}
                        onAction={async (key) => {
                           if (key === "sign-out") {
                              await handleSignOut();
                           }
                           if (key === "status") {
                              console.log("im here");
                              setIsDropdownMenuOpen(true);
                           }
                        }}
                        aria-label={"User actions"}
                     >
                        <DropdownSection showDivider>
                           <DropdownItem
                              textValue={"profile"}
                              classNames={{
                                 description: "text-[.75rem] text-default-300",
                              }}
                              description={"Manage your profile settings"}
                              className={`flex px-3 py-2 items-center gap-3`}
                              startContent={<ProfileIcon size={18} />}
                              key={"profile"}
                           >
                              <span className={`text-sm`}>Profile</span>
                           </DropdownItem>
                           {/*<DropdownItem*/}
                           {/*   endContent={*/}
                           {/*      <RightArrow*/}
                           {/*         className={`fill-default-400`}*/}
                           {/*         size={24}*/}
                           {/*      />*/}
                           {/*   }*/}
                           {/*   className={`flex px-3 py-2 items-center gap-3`}*/}
                           {/*   key={"status"}*/}
                           {/*>*/}
                           {/*   /!*<Select*!/*/}
                           {/*   /!*   placeholder={"Placeholder"}*!/*/}
                           {/*   /!*   variant={"flat"}*!/*/}
                           {/*   /!*   color={"default"}*!/*/}
                           {/*   /!*   id={"status-select"}*!/*/}
                           {/*   /!*   label={"Change your status"}*!/*/}
                           {/*   /!*>*!/*/}
                           {/*   /!*   <SelectItem value={"1"} key={"1"}>*!/*/}
                           {/*   /!*      Item 1*!/*/}
                           {/*   /!*   </SelectItem>*!/*/}
                           {/*   /!*   <SelectItem value={"2"} key={"1"}>*!/*/}
                           {/*   /!*      Item 2*!/*/}
                           {/*   /!*   </SelectItem>*!/*/}
                           {/*   /!*</Select>*!/*/}
                           {/*   Change your status*/}
                           {/*</DropdownItem>*/}
                           <DropdownItem
                              onClick={(e) => e.stopPropagation()}
                              textValue={"darkMode"}
                              endContent={
                                 <Switch
                                    size={"sm"}
                                    onChange={(e) => e.stopPropagation()}
                                    onClick={(e) => e.stopPropagation()}
                                    onValueChange={(_) => {
                                       setTheme(_ ? "dark" : "light");
                                       setIsDropdownMenuOpen(true);
                                    }}
                                    isSelected={theme === "dark"}
                                 />
                              }
                              className={`px-3 py-2`}
                              key={"darkMode"}
                           >
                              <span className={`text-small`}>Dark Mode</span>
                           </DropdownItem>
                        </DropdownSection>
                        <DropdownSection>
                           <DropdownItem
                              textValue={"sign-out"}
                              className={`flex px-3 py-2 items-center gap-3`}
                              startContent={
                                 <ExitIcon size={22} fill={"white"} />
                              }
                              color={"default"}
                              key={"sign-out"}
                           >
                              <span className={`text-small`}>Sign Out</span>
                           </DropdownItem>
                        </DropdownSection>
                        <DropdownSection>
                           <DropdownItem
                              onClick={(e) => {
                                 e.stopPropagation();
                                 setIsDropdownMenuOpen(true);
                              }}
                              isDisabled
                              textValue={"about"}
                              className={`cursor-default`}
                           >
                              <span
                                 className={`text-xs leading-2 break-words mt-6 text-default-500`}
                              >
                                 Chatify, Inc. © {new Date().getFullYear()}. All{" "}
                                 <br />
                                 rights reserved.
                              </span>
                           </DropdownItem>
                        </DropdownSection>
                     </DropdownMenu>
                  </Dropdown>
               </NavbarItem>
            ) : (
               NAV_LINKS.map((link, i) => (
                  <NavbarItem isActive={pathname === link.href} key={i}>
                     <Link
                        size={"lg"}
                        color={(link.color as any) ?? "foreground"}
                        className={`text-foreground group-[data-active=true]:text-primary`}
                        href={link.href}
                     >
                        {link.label}
                     </Link>
                  </NavbarItem>
               ))
            )}
         </NavbarContent>
      </Navbar>
   );
};

export default MainNavbar;
