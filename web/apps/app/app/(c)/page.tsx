"use client";
import React, { Fragment } from "react";
import Link from "next/link";
import { useGetChatGroupDetailsQuery, useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "@hooks";
import {
   Button,
   Modal,
   ModalBody,
   ModalContent,
   ModalHeader,
   useDisclosure,
} from "@nextui-org/react";
import { useCurrentChatGroup } from "@hooks";
import ChatGroupTopBar from "@components/chat-group/ChatGroupTopBar";
import { useSearchParams } from "next/navigation";
import CrossIcon from "@components/icons/CrossIcon";
import { ChatMessagesSection } from "@components/chat-group";
import { useChatifyClientContext } from "../../hub/ChatHubConnection";
import { X } from "lucide-react";
import Toast from "@components/common/Toast";

export const revalidate = 0;

function IndexPage(props) {
   const params = useSearchParams();
   const isNew = params.get("new") === "true";
   const { data: me } = useGetMyClaimsQuery();

   const client = useChatifyClientContext();
   const {
      isOpen: isSuccessModalOpen,
      onOpenChange: onSuccessModalOpenChange,
   } = useDisclosure({ defaultOpen: true });

   const chatGroupId = useCurrentChatGroup();
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const {
      data: chatGroupDetails,
      error,
      isLoading,
   } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId && isUserLoggedIn,
   });

   const handleSignalRTest = async () => {
      await client.test(
         chatGroupId,
         `Test value from ${me.claims.nameidentifier}`
      );
   };

   return (
      <div
         className={`flex min-h-[60vh] text-xl flex-col items-center shadow-gray-300 w-full rounded-md mb-6`}
      >
         <ChatGroupTopBar />
         {isNew && chatGroupDetails && (
            <Toast
               className={`bg-success-300`}
               header={"Success!"}
               isOpen={isSuccessModalOpen}
               onOpenChange={onSuccessModalOpenChange}
            >
               <p>
                  You successfully created{" "}
                  <strong className={`text-small mr-1`}>
                     {chatGroupDetails?.chatGroup?.name}
                  </strong>
                  chat group.
               </p>
            </Toast>
         )}
         {!isUserLoggedIn ? (
            <UserNotLoggedInSection />
         ) : (
            <div
               className={`self-center w-full flex flex-col items-center mt-4`}
            >
               <ChatMessagesSection groupId={chatGroupId} />
            </div>
         )}
      </div>
   );
}

const UserNotLoggedInSection = () => (
   <div className={`mt-12`}>
      <h2 className={`font-bold`}>You are currently not logged in.</h2>
      <div className={`flex justify-center items-center gap-8`}>
         <Link className={`hover:underline text-blue-600`} href={`/signup`}>
            {" "}
            Sign Up
         </Link>
         <Link className={`hover:underline text-blue-600`} href={`/signin`}>
            Sign In
         </Link>
      </div>
   </div>
);

export default IndexPage;
