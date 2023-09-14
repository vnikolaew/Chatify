"use client";
import React, {
   Fragment,
   useCallback,
   useEffect,
   useRef,
   useState,
} from "react";
import {
   Avatar,
   Button,
   Chip,
   Input,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Spinner,
} from "@nextui-org/react";
import { useForwardChatMessage, useSearchChatGroupsByName } from "@web/api";
import { ChatGroup, ChatMessage } from "@openapi";
import { useDebounce } from "@hooks";
import ChatGroupSearchEntries from "@components/chat-group/messages/ChatGroupSearchEntries";
import { X } from "lucide-react";

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
      refetch,
      error: searchError,
   } = useSearchChatGroupsByName(
      { query: debouncedSearch },
      { enabled: debouncedSearch?.length >= 2 }
   );
   const [selectedGroups, setSelectedGroups] = useState([]);
   const inputRef = useRef<HTMLInputElement>();
   const focusInput = useCallback(() => inputRef.current?.focus(), []);
   const [isInputFocused, setIsInputFocused] = useState(false);
   useEffect(() => focusInput(), []);

   return (
      <Modal
         shadow={"md"}
         radius={"sm"}
         placement={`center`}
         size={`lg`}
         onOpenChange={onOpenChange}
         isOpen={isOpen}
      >
         <ModalContent className={`overflow-visible`}>
            {(onClose) => (
               <Fragment>
                  <ModalHeader>Forward this message</ModalHeader>
                  <ModalBody>
                     <div className={`relative`}>
                        <Input
                           color={`default`}
                           value={groupSearchQuery}
                           isClearable
                           ref={inputRef}
                           onFocusChange={setIsInputFocused}
                           onFocus={async (_) => {
                              if (!searchEntries?.length) {
                                 await refetch({});
                              }
                           }}
                           startContent={
                              !!selectedGroups.length ? (
                                 <div className={`flex items-center gap-1`}>
                                    {selectedGroups.map((group: ChatGroup) => (
                                       <Chip
                                          size={`sm`}
                                          classNames={{
                                             content: `px-2 font-normal text-xs`,
                                          }}
                                          startContent={
                                             <Avatar
                                                src={group.picture.mediaUrl}
                                                className={`w-4 h-4`}
                                             />
                                          }
                                          endContent={
                                             <X
                                                onClick={(_) => {
                                                   focusInput();
                                                   setSelectedGroups((g) =>
                                                      g.filter(
                                                         (g) =>
                                                            g.id !== group.id
                                                      )
                                                   );
                                                }}
                                                className={`stroke-default-400 mr-1 transition-all duration-100 hover:stroke-default-600 cursor-pointer`}
                                                size={12}
                                             />
                                          }
                                          key={group.id}
                                       >
                                          {group.name}
                                       </Chip>
                                    ))}
                                 </div>
                              ) : (
                                 null!
                              )
                           }
                           onValueChange={setGroupSearchQuery}
                           variant={`bordered`}
                           size={`md`}
                           radius={`sm`}
                           className={`w-full`}
                           classNames={{
                              input: `pl-2`,
                           }}
                           placeholder={`Search for a group or a person`}
                           type={`text`}
                        />
                        {(searchEntries?.length > 0 ?? false) &&
                           isInputFocused && (
                              <ChatGroupSearchEntries
                                 loading={searchLoading && searchFetching}
                                 onSelect={(group) => {
                                    setSelectedGroups((g) => [...g, group]);
                                    // setGroupSearchQuery(``);
                                    focusInput();
                                 }}
                                 entries={searchEntries}
                              />
                           )}
                     </div>
                  </ModalBody>
                  <ModalFooter className={`mt-4`}>
                     <Button
                        variant={`bordered`}
                        radius={`sm`}
                        size={`md`}
                        color={`default`}
                        onPress={onClose}
                     >
                        Save Draft
                     </Button>
                     <Button
                        className={`ml-2`}
                        isLoading={isLoading}
                        isDisabled={isLoading}
                        spinner={<Spinner color={`white`} size={`sm`} />}
                        variant={`solid`}
                        radius={`sm`}
                        size={`md`}
                        color={`success`}
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