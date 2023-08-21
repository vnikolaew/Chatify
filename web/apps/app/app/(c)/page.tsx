"use client";
import React, { Fragment, useState } from "react";
import Link from "next/link";
import SignOut from "../../components/SignOut";
import { useSearchParams } from "next/navigation";
import { useGetChatGroupDetailsQuery, useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "../../hooks/useIsUserLoggedIn";
import { Avatar, Button, Skeleton, Tooltip } from "@nextui-org/react";
import PinIcon from "../../components/icons/PinIcon";
import AddUserIcon from "../../components/icons/AddUserIcon";

export const revalidate = 0;

function IndexPage(props) {
   const [tooltipsOpen, setTooltipsOpen] = useState<Record<string, boolean>>(
      {}
   );
   const params = useSearchParams();
   const chatGroupId = params.get("c");
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const {
      data: chatGroupDetails,
      error,
      isLoading,
   } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId && isUserLoggedIn,
   });
   const { data: me, error: meError } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });

   console.log(chatGroupId);
   console.log(me?.claims?.nameidentifier);
   console.log(chatGroupDetails);

   return (
      <div
         className={`flex min-h-[60vh] text-xl gap-3 flex-col items-center shadow-gray-300 w-full rounded-md mb-6`}
      >
         <div
            className={`flex border-b-1 border-b-default-200 w-full items-center justify-between p-3 gap-2`}
         >
            <div className={`flex h-full ml-4 items-start gap-4`}>
               <Avatar
                  fallback={<Skeleton className={`h-10 w-10 rounded-full`} />}
                  isBordered
                  radius={"full"}
                  color={"danger"}
                  size={"md"}
                  className={`aspect-square object-cover`}
                  src={chatGroupDetails?.chatGroup?.picture?.mediaUrl}
               />
               <div
                  className={`flex flex-col items-start justify-evenly h-full`}
               >
                  {isLoading ? (
                     <>
                        <Skeleton
                           as={"div"}
                           className={`text-medium rounded-full w-20 h-4 text-foreground`}
                        />
                        <Skeleton
                           as={"div"}
                           className={`text-small rounded-full w-10 h-2 text-default-500`}
                        />
                     </>
                  ) : (
                     <>
                        <span className={`text-medium text-foreground`}>
                           {" "}
                           {chatGroupDetails.chatGroup.name}
                        </span>
                        <span className={`text-xs text-default-400`}>
                           {chatGroupDetails.members.length} members
                        </span>
                     </>
                  )}
               </div>
            </div>
            <div className="mx-2 flex items-center gap-1">
               <Tooltip
                  isOpen={tooltipsOpen.pin}
                  onOpenChange={(open) =>
                     setTooltipsOpen((s) => ({ ...s, pin: open }))
                  }
                  shadow={"md"}
                  classNames={{
                     base: "text-xs",
                  }}
                  showArrow
                  color={"default"}
                  placement={"bottom"}
                  offset={2}
                  size={"md"}
                  content={"Pinned messages"}
               >
                  <Button
                     className={`bg-transparent duration-200 hover:bg-default-200`}
                     radius={"full"}
                     color={"default"}
                     isIconOnly
                  >
                     <PinIcon fill={"white"} size={20} />
                  </Button>
               </Tooltip>
               {true && (
                  // chatGroupDetails?.chatGroup?.adminIds?.some( (id) => id === me.claims.nameidentifier
                  <Tooltip
                     isOpen={tooltipsOpen.addMember}
                     onOpenChange={(open) =>
                        setTooltipsOpen((s) => ({ ...s, addMember: open }))
                     }
                     shadow={"md"}
                     color={"primary"}
                     size={"md"}
                     content={"Add a new member"}
                     placement={"bottom"}
                  >
                     <Button
                        className={`bg-transparent duration-200 hover:bg-default-200`}
                        radius={"full"}
                        color={"default"}
                        isIconOnly
                     >
                        <AddUserIcon fill={"white"} size={22} />
                     </Button>
                  </Tooltip>
               )}
            </div>
         </div>
         <h1 className={`text-3xl my-4`}>
            {chatGroupDetails?.chatGroup?.name}
         </h1>
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
