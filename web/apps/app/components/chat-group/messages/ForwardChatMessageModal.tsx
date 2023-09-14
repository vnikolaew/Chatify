"use client";
import React, { Fragment, useCallback, useState } from "react";
import {
   Avatar,
   Button,
   Input,
   Listbox,
   ListboxItem,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Spinner,
} from "@nextui-org/react";
import {
   getMediaUrl,
   useForwardChatMessage,
   useGetMyFriendsQuery,
   useSearchChatGroupsByName,
} from "@web/api";
import { ChatGroup, ChatMessage } from "@openapi";
import { useCurrentUserId, useDebounce } from "@hooks";
import ChatGroupSearchEntry from "@components/chat-group/messages/ChatGroupSearchEntry";

export interface ForwardChatMessageModalProps {
   isOpen: boolean;
   onOpenChange: () => void;
   message: ChatMessage;
}

const ForwardChatMessageModal = ({
   onOpenChange,
   message,
   isOpen,
}: ForwardChatMessageModalProps) => {
   const {
      mutateAsync: forwardMessage,
      isLoading,
      error,
   } = useForwardChatMessage();
   const [groupSearchQuery, setGroupSearchQuery] = useState(``);
   const debouncedSearch = useDebounce(groupSearchQuery, 2000);
   const {
      data: searchEntries,
      isLoading: searchLoading,
      isFetching: searchFetching,
      error: searchError,
   } = useSearchChatGroupsByName(
      { query: debouncedSearch },
      { enabled: debouncedSearch?.length >= 2 }
   );

   console.log(searchEntries);
   return (
      <Modal
         shadow={"md"}
         radius={"sm"}
         placement={`center`}
         size={`sm`}
         onOpenChange={onOpenChange}
         isOpen={isOpen}
      >
         <ModalContent className={`overflow-visible`}>
            {(onClose) => (
               <Fragment>
                  <ModalHeader>
                     Forward this message - {message?.id}
                  </ModalHeader>
                  <ModalBody>
                     <div className={`relative`}>
                        <Input
                           color={`default`}
                           value={groupSearchQuery}
                           isClearable
                           onValueChange={setGroupSearchQuery}
                           variant={`bordered`}
                           size={`md`}
                           radius={`sm`}
                           className={`w-full`}
                           placeholder={`Search for a group or a person`}
                           type={`text`}
                        />
                        {groupSearchQuery.length > 0 &&
                           (searchEntries?.length > 0 ?? false) && (
                              <Listbox
                                 // color={`primary`}
                                 variant={`solid`}
                                 className={`absolute rounded-medium bg-default-100 bg-opacity-100 z-[100] bottom-0 translate-y-[105%]`}
                                 items={searchEntries ?? []}
                                 onAction={console.log}
                                 aria-label={`Search entries`}
                              >
                                 {(item) => (
                                    <ChatGroupSearchEntry
                                       key={item.id}
                                       chatGroup={item as ChatGroup}
                                    />
                                 )}
                              </Listbox>
                           )}
                     </div>
                  </ModalBody>
                  <ModalFooter className={`mt-2`}>
                     <Button
                        variant={`solid`}
                        radius={`sm`}
                        size={`sm`}
                        color={`danger`}
                        onPress={onClose}
                     >
                        Cancel
                     </Button>
                     <Button
                        className={`ml-2`}
                        isLoading={isLoading}
                        isDisabled={isLoading}
                        spinner={<Spinner color={`white`} size={`sm`} />}
                        variant={`solid`}
                        radius={`sm`}
                        size={`sm`}
                        color={`default`}
                        onPress={async (_) => {
                           await forwardMessage({
                              messageId: message.id,
                              groupId: message.chatGroupId,
                              content: ``,
                           });
                           onClose();
                        }}
                     >
                        {isLoading ? "Loading ..." : `Forward`}
                     </Button>
                  </ModalFooter>
               </Fragment>
            )}
         </ModalContent>
      </Modal>
   );
};

export default ForwardChatMessageModal;
