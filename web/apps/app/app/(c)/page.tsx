"use client";
import React, { Fragment, useState } from "react";
import Link from "next/link";
import SignOut from "../../components/SignOut";
import { useGetChatGroupDetailsQuery, useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "../../hooks/useIsUserLoggedIn";
import {
   Avatar,
   AvatarGroup,
   Button,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Skeleton,
   useDisclosure,
} from "@nextui-org/react";
import PinIcon from "../../components/icons/PinIcon";
import AddUserIcon from "../../components/icons/AddUserIcon";
import TooltipButton from "../../components/TooltipButton";
import { useCurrentChatGroup } from "../../hooks/chat-groups/useCurrentChatGroup";
import { UserStatus } from "@openapi/models/UserStatus";

export const revalidate = 0;

function IndexPage(props) {
   const [tooltipsOpen, setTooltipsOpen] = useState<Record<string, boolean>>(
      {}
   );
   const chatGroupId = useCurrentChatGroup();
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

   const { isOpen, onOpenChange, onOpen } = useDisclosure({
      defaultOpen: false,
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
               {chatGroupDetails ? (
                  <Fragment>
                     <Avatar
                        fallback={
                           <Skeleton className={`h-10 w-10 rounded-full`} />
                        }
                        isBordered
                        radius={"full"}
                        color={"danger"}
                        size={"md"}
                        className={`aspect-square object-cover`}
                        src={chatGroupDetails?.chatGroup?.picture?.mediaUrl}
                     />
                     <div
                        className={`flex flex-col items-start justify-around h-full`}
                     >
                        {isLoading ? (
                           <Fragment>
                              <Skeleton
                                 as={"div"}
                                 className={`text-medium rounded-full w-20 h-4 text-foreground`}
                              />
                              <Skeleton
                                 as={"div"}
                                 className={`text-small rounded-full w-10 h-2 text-default-500`}
                              />
                           </Fragment>
                        ) : (
                           <Fragment>
                              <span className={`text-medium text-foreground`}>
                                 {" "}
                                 {chatGroupDetails.chatGroup.name}
                              </span>
                              <div className={`flex items-center gap-3`}>
                                 <span className={`text-xs text-default-400`}>
                                    {chatGroupDetails.members.length} members
                                 </span>

                                 <div
                                    className={`bg-default-200 rounded-full w-[4px] h-[4px]`}
                                 />
                                 <div className={`flex items-center gap-2`}>
                                    <AvatarGroup
                                       renderCount={(count) => (
                                          <div
                                             className={`w-5 flex items-end text-[.5rem] h-5 rounded-full bg-default-300 justify-center`}
                                          >
                                             +{count}
                                          </div>
                                       )}
                                       size={"sm"}
                                       className={`h-3`}
                                       max={3}
                                    >
                                       {chatGroupDetails.members
                                          .filter(
                                             (_) =>
                                                _.status === UserStatus.ONLINE
                                          )
                                          .map((user, i) => (
                                             <Avatar
                                                classNames={{
                                                   base: "w-4 h-4",
                                                }}
                                                size={"sm"}
                                                color={"success"}
                                                radius={"full"}
                                                className={`w-4 h-4`}
                                                src={
                                                   user.profilePicture.mediaUrl
                                                }
                                                key={user.id}
                                             />
                                          ))}
                                    </AvatarGroup>
                                    <span
                                       className={`text-xs text-success-300`}
                                    >
                                       {
                                          chatGroupDetails.members.filter(
                                             (m) =>
                                                m.status === UserStatus.ONLINE
                                          ).length
                                       }{" "}
                                       online
                                    </span>
                                 </div>
                              </div>
                           </Fragment>
                        )}
                     </div>
                  </Fragment>
               ) : (
                  <div>
                     <span className={`text-default-200 text-medium`}>
                        There's no chat group selected.
                     </span>
                  </div>
               )}
            </div>
            <div className="mx-2 flex items-center gap-2">
               <TooltipButton
                  content={"Pinned messages"}
                  icon={<PinIcon fill={"white"} size={24} />}
               />
               {chatGroupDetails?.chatGroup?.adminIds?.some(
                  (id) => id === me.claims.nameidentifier
               ) && (
                  <TooltipButton
                     content={"Add a new member"}
                     icon={<AddUserIcon fill={"white"} size={24} />}
                  />
               )}
            </div>
         </div>
         <h1 className={`text-3xl my-4`}>
            {chatGroupDetails?.chatGroup?.name}
         </h1>
         <div>
            <Button
               onPress={onOpen}
               size={"md"}
               variant={"shadow"}
               color={"primary"}
            >
               Open modal
            </Button>
         </div>
         <Modal
            backdrop={"transparent"}
            classNames={{
               base: "absolute right-10 bg-default-200 -bottom-10",
            }}
            shadow={"md"}
            radius={"sm"}
            motionProps={{
               variants: {
                  enter: {
                     y: 0,
                     opacity: 1,
                     transition: {
                        duration: 0.3,
                        ease: "easeOut",
                     },
                  },
                  exit: {
                     y: 20,
                     opacity: 0,
                     transition: {
                        duration: 0.2,
                     },
                  },
               },
            }}
            size={"md"}
            placement={"bottom"}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
         >
            <ModalContent>
               <ModalHeader>Header</ModalHeader>
               <ModalBody>Body</ModalBody>
               <ModalFooter>Footer</ModalFooter>
            </ModalContent>
         </Modal>
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
