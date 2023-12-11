"use client";
import {
   Blockquote,
   Box,
   Container,
   Flex,
   Heading,
   Text,
   Code,
   Em,
   Tabs,
   Kbd,
   Quote,
   Strong,
   AlertDialog,
   Button, AspectRatio, Avatar, Badge, Callout, Card, Inset, Checkbox, ContextMenu, Dialog, TextField, DropdownMenu, HoverCard, Link, IconButton, Popover, TextArea, ScrollArea, Select, Slider, Tooltip,
} from "@radix-ui/themes";
import React from "react";
import { Image } from "@nextui-org/react";
import { BookmarkIcon, ChevronDownIcon, InfoIcon, PlusIcon, SearchIcon } from "lucide-react";
import ChatBubbleIcon from "@components/icons/ChatBubbleIcon";

export interface PageProps {

}

const Page = ({}: PageProps) => {
   return (
      <Container size={`4`} className={`mt-8 w-full`}>
         <Flex align={`center`} gap={`4`} direction={`column`}>
            <Heading className={`text-center`}>
               Radix-UI Themes
            </Heading>
            <Tabs.Root defaultValue={`3`}>
               <Tabs.List size={`2`} className={`mx-auto flex items-center justify-center`}>
                  <Tabs.Trigger value={`1`}>1</Tabs.Trigger>
                  <Tabs.Trigger value={`2`}>2</Tabs.Trigger>
                  <Tabs.Trigger value={`3`}>3</Tabs.Trigger>
               </Tabs.List>

               <Tabs.Content className={`flex flex-col items-center gap-2 mt-3`} value={`1`}>
                  <Box width={"9"} height={"9"}>
                     I'm a box
                  </Box>
                  <Flex gap={`0`} align={`start`} direction={`column`}>
                     <Text color={`amber`} weight={`medium`} size={`4`}>The quick brown fox jumps over the lazy
                        dog.</Text>
                     <Text color={`gray`} weight={`light`} size={`2`}>Small description.</Text>
                  </Flex>
                  <Flex gap={`0`} align={`start`} direction={`column`}>
                     <Heading color={`indigo`} highContrast as={`h1`} size={`7`}>I'm a heading</Heading>
                     <Heading color={`bronze`} as={`h2`} size={`5`}>I'm a smaller heading 2</Heading>
                     <Heading color={`jade`} as={`h3`} size={`3`}>I'm the smallest heading 3</Heading>
                  </Flex>
                  <Flex gap={`1`} align={`start`} direction={`column`}>
                     <Blockquote weight={`light`} size={`3`}>Some wise words here.</Blockquote>
                     <Blockquote color={`crimson`} size={`5`}>Another wise words here.</Blockquote>
                  </Flex>
                  <Flex gap={`1`} align={`start`} direction={`column`}>
                     <Code variant={`ghost`} color={`lime`} size={`4`}>Some code.</Code>
                     <Code highContrast={true} variant={`outline`} color={`gray`} size={`4`}>Some code.</Code>
                     <Code weight={`bold`} variant={`solid`} color={`crimson`} size={`2`}>Some code.</Code>
                     <Code variant={`soft`} color={`cyan`} size={`5`}>Some code.</Code>
                  </Flex>
                  <Flex gap={`1`} align={`start`} direction={`column`}>
                     <Text> Some text with a <Em>em</Em> word.</Text>
                     <Text> Some text with a <Kbd size={`3`}>Keyboard</Kbd> word.</Text>
                  </Flex>
                  <Flex gap={`1`} align={`start`} direction={`column`}>
                     <Text>
                        His famous quote,{" "}
                        <Quote>Styles come and go. Good design is a language, not a <Strong>style</Strong> </Quote>,
                        elegantly summs up Massimo’s philosophy of design.
                     </Text>
                  </Flex>
                  <Flex gap={`1`} align={`start`} direction={`column`}>
                     <AlertDialog.Root>
                        <AlertDialog.Trigger>
                           <Button variant={`classic`} color={`jade`}>Revoke access</Button>
                        </AlertDialog.Trigger>
                        <AlertDialog.Content size={`4`} style={{ maxWidth: 450 }}>
                           <AlertDialog.Title>Revoke access</AlertDialog.Title>
                           <AlertDialog.Description size="2">
                              Are you sure? This application will no longer be accessible and any
                              existing sessions will be expired.
                           </AlertDialog.Description>

                           <Flex gap="3" mt="4" justify="end">
                              <AlertDialog.Cancel>
                                 <Button variant="soft" color="gray">
                                    Cancel
                                 </Button>
                              </AlertDialog.Cancel>
                              <AlertDialog.Action>
                                 <Button variant="solid" color="red">
                                    Revoke access
                                 </Button>
                              </AlertDialog.Action>
                           </Flex>
                        </AlertDialog.Content>
                     </AlertDialog.Root>
                  </Flex>
                  <Flex gap={`1`} align={`start`} direction={`column`}>
                     <AspectRatio ratio={16 / 8}>
                        <Image
                           width={100}
                           height={100}
                           src="https://images.unsplash.com/photo-1479030160180-b1860951d696?&auto=format&fit=crop&w=1200&q=80"
                           alt="A house in a forest"
                           style={{
                              objectFit: "cover",
                              width: "100%",
                              height: "100%",
                              borderRadius: "var(--radius-2)",
                           }}
                        />
                     </AspectRatio>
                  </Flex>
               </Tabs.Content>
               <Tabs.Content className={`flex flex-col items-center gap-2 mt-3`} value={`2`}>
                  <Flex gap="2">
                     <Avatar
                        src="https://images.unsplash.com/photo-1502823403499-6ccfcf4fb453?&w=256&h=256&q=70&crop=focalpoint&fp-x=0.5&fp-y=0.3&fp-z=1&fit=crop"
                        fallback="A"
                     />
                     <Avatar radius={`large`} color={`crimson`} variant={`soft`} size={`4`} fallback="A" />
                     <Avatar radius={`full`} color={`amber`} variant={`solid`} size={`5`} fallback="A" />
                     <Avatar radius={`full`} color={`jade`} variant={`solid`} size={`3`} fallback="A" />
                  </Flex>
                  <Flex gap={`2`} align={`start`} direction={`column`}>
                     <Badge variant={`surface`} radius={`large`} size={`2`} color="orange">In progress</Badge>
                     <Badge variant={`soft`} color="blue">In review</Badge>
                     <Badge variant={`outline`} color="green">Complete</Badge>
                  </Flex>
                  <Flex gap={`2`} align={`start`} direction={`column`}>
                     <Button size={`3`} variant={`ghost`} color={`lime`}>
                        <BookmarkIcon width="16" height="16" /> Bookmark
                     </Button>
                  </Flex>
                  <Flex gap={`2`} align={`start`} direction={`column`}>
                     <Callout.Root role={`alert`} color={`green`} variant={`soft`} size={`2`}>
                        <Callout.Icon>
                           <InfoIcon />
                        </Callout.Icon>
                        <Callout.Text>
                           You will need admin privileges to install and access this application.
                        </Callout.Text>
                     </Callout.Root>
                     <Card size="2" style={{ maxWidth: 210 }}>
                        <Inset className={`w-full`} clip="padding-box" side="top" pb="current">
                           <Image
                              src="https://images.unsplash.com/photo-1617050318658-a9a3175e34cb?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=600&q=80"
                              alt="Bold typography"
                              style={{
                                 display: "block",
                                 objectFit: "cover",
                                 width: "100%",
                                 height: 140,
                                 backgroundColor: "var(--gray-5)",
                              }}
                           />
                        </Inset>
                        <Text as="p" size="3">
                           <Strong>Typography</Strong> is the art and technique of arranging type to
                           make written language legible, readable and appealing when displayed.
                        </Text>
                     </Card>
                  </Flex>
                  <Flex gap={`2`} align={`start`} direction={`column`}>
                     <Text as="label" size="2">
                        <Flex gap="2">
                           <Checkbox color={`crimson`} defaultChecked /> Agree to Terms and Conditions
                        </Flex>
                     </Text>
                  </Flex>
                  <Flex gap={`2`} align={`start`} direction={`column`}>
                     <ContextMenu.Root>
                        <ContextMenu.Trigger>
                           <Box width={`100%`} style={{ height: 150 }}>
                              Right-click here
                           </Box>
                        </ContextMenu.Trigger>
                        <ContextMenu.Content>
                           <ContextMenu.Item shortcut="⌘ E">Edit</ContextMenu.Item>
                           <ContextMenu.Item shortcut="⌘ D">Duplicate</ContextMenu.Item>
                           <ContextMenu.Separator />
                           <ContextMenu.Item shortcut="⌘ N">Archive</ContextMenu.Item>

                           <ContextMenu.Sub>
                              <ContextMenu.SubTrigger>More</ContextMenu.SubTrigger>
                              <ContextMenu.SubContent>
                                 <ContextMenu.Item>Move to project…</ContextMenu.Item>
                                 <ContextMenu.Item>Move to folder…</ContextMenu.Item>
                                 <ContextMenu.Separator />
                                 <ContextMenu.Item>Advanced options…</ContextMenu.Item>
                              </ContextMenu.SubContent>
                           </ContextMenu.Sub>

                           <ContextMenu.Separator />
                           <ContextMenu.Item>Share</ContextMenu.Item>
                           <ContextMenu.Item>Add to favorites</ContextMenu.Item>
                           <ContextMenu.Separator />
                           <ContextMenu.Item shortcut="⌘ ⌫" color="red">
                              Delete
                           </ContextMenu.Item>
                        </ContextMenu.Content>
                     </ContextMenu.Root>
                  </Flex>
               </Tabs.Content>

               <Tabs.Content className={`flex flex-col items-center gap-2 mt-3`} value={`3`}>
                  <Flex gap={`4`} align={`center`} direction={`column`}>
                     <Dialog.Root>
                        <Dialog.Trigger>
                           <Button variant={`classic`} color={`gold`}>Edit profile</Button>
                        </Dialog.Trigger>

                        <Dialog.Content size={`3`} style={{ maxWidth: 450 }}>
                           <Dialog.Title>Edit profile</Dialog.Title>
                           <Dialog.Description size="2" mb="4">
                              Make changes to your profile.
                           </Dialog.Description>

                           <Flex direction="column" gap="3">
                              <label>
                                 <Text as="div" size="2" mb="1" weight="bold">
                                    Name
                                 </Text>
                                 <TextField.Input

                                    defaultValue="Freja Johnsen"
                                    placeholder="Enter your full name"
                                 />
                              </label>
                              <label>
                                 <Text as="div" size="2" mb="1" weight="bold">
                                    Email
                                 </Text>
                                 <TextField.Input
                                    defaultValue="freja@example.com"
                                    placeholder="Enter your email"
                                 />
                              </label>
                           </Flex>

                           <Flex gap="3" mt="4" justify="end">
                              <Dialog.Close>
                                 <Button variant="soft" color="gray">
                                    Cancel
                                 </Button>
                              </Dialog.Close>
                              <Dialog.Close>
                                 <Button variant={`surface`} color={`blue`}>Save</Button>
                              </Dialog.Close>
                           </Flex>
                        </Dialog.Content>
                     </Dialog.Root>
                     <DropdownMenu.Root>
                        <DropdownMenu.Trigger>
                           <Button color={`blue`} variant={`surface`}>
                              Options
                              <ChevronDownIcon size={16} />
                           </Button>
                        </DropdownMenu.Trigger>
                        <DropdownMenu.Content color={`indigo`} variant={`soft`}>
                           <DropdownMenu.Item shortcut="⌘ E">Edit</DropdownMenu.Item>
                           <DropdownMenu.Item shortcut="⌘ D">Duplicate</DropdownMenu.Item>
                           <DropdownMenu.Separator />
                           <DropdownMenu.Item shortcut="⌘ N">Archive</DropdownMenu.Item>

                           <DropdownMenu.Sub>
                              <DropdownMenu.SubTrigger>More</DropdownMenu.SubTrigger>
                              <DropdownMenu.SubContent>
                                 <DropdownMenu.Item>Move to project…</DropdownMenu.Item>
                                 <DropdownMenu.Item>Move to folder…</DropdownMenu.Item>

                                 <DropdownMenu.Separator />
                                 <DropdownMenu.Item>Advanced options…</DropdownMenu.Item>
                              </DropdownMenu.SubContent>
                           </DropdownMenu.Sub>

                           <DropdownMenu.Separator />
                           <DropdownMenu.Item>Share</DropdownMenu.Item>
                           <DropdownMenu.Item>Add to favorites</DropdownMenu.Item>
                           <DropdownMenu.Separator />
                           <DropdownMenu.Item shortcut="⌘ ⌫" color="red">
                              Delete
                           </DropdownMenu.Item>
                        </DropdownMenu.Content>
                     </DropdownMenu.Root>
                     <Text>
                        Follow{" "}
                        <HoverCard.Root>
                           <HoverCard.Trigger>
                              <Link href="https://twitter.com/radix_ui" target="_blank">
                                 @radix_ui
                              </Link>
                           </HoverCard.Trigger>
                           <HoverCard.Content>
                              <Flex gap="4">
                                 <Avatar
                                    size="3"
                                    fallback="R"
                                    radius="full"
                                    src="https://pbs.twimg.com/profile_images/1337055608613253126/r_eiMp2H_400x400.png"
                                 />
                                 <Box>
                                    <Heading size="3" as="h3">
                                       Radix
                                    </Heading>
                                    <Text as="div" size="2" color="gray">
                                       @radix_ui
                                    </Text>

                                    <Text as="div" size="2" style={{ maxWidth: 300 }} mt="3">
                                       React components, icons, and colors for building high-quality,
                                       accessible UI.
                                    </Text>
                                 </Box>
                              </Flex>
                           </HoverCard.Content>
                        </HoverCard.Root>{" "}
                        for updates.
                     </Text>
                     <Flex gap={`2`} direction={`row`}>
                        <IconButton onClick={console.log} className={`cursor-pointer`} radius={`full`} color={`crimson`} variant={`surface`} size={`3`}>
                           <SearchIcon width="18" height="18" />
                        </IconButton>
                        <IconButton onClick={console.log} className={`cursor-pointer`} radius={`full`} color={`jade`} variant={`outline`} size={`3`}>
                           <SearchIcon width="18" height="18" />
                        </IconButton>
                        <IconButton onClick={console.log} className={`cursor-pointer`} radius={`full`} color={`blue`} variant={`solid`} size={`3`}>
                           <SearchIcon width="18" height="18" />
                        </IconButton>
                     </Flex>
                     <Popover.Root>
                        <Popover.Trigger>
                           <Button color={`gold`} variant={`soft`}>
                              <ChatBubbleIcon size={16} width="16" height="16" />
                              Comment
                           </Button>
                        </Popover.Trigger>
                        <Popover.Content style={{ width: 360 }}>
                           <Flex gap="3">
                              <Avatar
                                 size="2"
                                 src="https://images.unsplash.com/photo-1607346256330-dee7af15f7c5?&w=64&h=64&dpr=2&q=70&crop=focalpoint&fp-x=0.67&fp-y=0.5&fp-z=1.4&fit=crop"
                                 fallback="A"
                                 radius="full"
                              />
                              <Box grow="1">
                                 <TextArea placeholder="Write a comment…" style={{ height: 80 }} />
                                 <Flex gap="3" mt="3" justify="between">
                                    <Flex align="center" gap="2" asChild>
                                       <Text as="label" size="2">
                                          <Checkbox />
                                          <Text>Send to group</Text>
                                       </Text>
                                    </Flex>

                                    <Popover.Close>
                                       <Button size="1">Comment</Button>
                                    </Popover.Close>
                                 </Flex>
                              </Box>
                           </Flex>
                        </Popover.Content>
                     </Popover.Root>
                     <ScrollArea className={`w-1/2 mx-auto`} type="always" scrollbars="vertical" style={{ height: 180 }}>
                        <Box p="2" pr="8">
                           <Heading size="4" mb="2" trim="start">
                              Principles of the typographic craft
                           </Heading>
                           <Flex direction="column" gap="4">
                              <Text as="p">
                                 Three fundamental aspects of typography are legibility, readability, and
                                 aesthetics. Although in a non-technical sense “legible” and “readable”
                                 are often used synonymously, typographically they are separate but
                                 related concepts.
                              </Text>

                              <Text as="p">
                                 Legibility describes how easily individual characters can be
                                 distinguished from one another. It is described by Walter Tracy as “the
                                 quality of being decipherable and recognisable”. For instance, if a “b”
                                 and an “h”, or a “3” and an “8”, are difficult to distinguish at small
                                 sizes, this is a problem of legibility.
                              </Text>

                              <Text as="p">
                                 Typographers are concerned with legibility insofar as it is their job to
                                 select the correct font to use. Brush Script is an example of a font
                                 containing many characters that might be difficult to distinguish. The
                                 selection of cases influences the legibility of typography because using
                                 only uppercase letters (all-caps) reduces legibility.
                              </Text>
                           </Flex>
                        </Box>
                     </ScrollArea>
                     <Select.Root defaultValue="apple">
                        <Select.Trigger color={`indigo`} variant={`ghost`} />
                        <Select.Content position={`popper`}>
                           <Select.Group>
                              <Select.Label>Fruits</Select.Label>
                              <Select.Item value="orange">Orange</Select.Item>
                              <Select.Item value="apple">Apple</Select.Item>
                              <Select.Item value="grape" disabled>
                                 Grape
                              </Select.Item>
                           </Select.Group>
                           <Select.Separator />
                           <Select.Group>
                              <Select.Label>Vegetables</Select.Label>
                              <Select.Item value="carrot">Carrot</Select.Item>
                              <Select.Item value="potato">Potato</Select.Item>
                           </Select.Group>
                        </Select.Content>
                     </Select.Root>
                     <Slider color={`crimson`} variant={`surface`} className={`w-1/2`} defaultValue={[50]} />
                     <Tooltip color={`jade`} content="Add to library">
                        <IconButton radius="full">
                           <PlusIcon size={12} />
                        </IconButton>
                     </Tooltip>
                     <TextField.Root variant={`soft`} color={`green`}>
                        <TextField.Slot>
                           <SearchIcon height="12" width="12" />
                        </TextField.Slot>
                        <TextField.Input placeholder="Search the docs…" />
                     </TextField.Root>
                     <TextField.Root variant={`classic`} color={`green`}>
                        <TextField.Slot>
                           <SearchIcon height="12" width="12" />
                        </TextField.Slot>
                        <TextField.Input placeholder="Search the docs…" />
                     </TextField.Root>
                     <TextField.Root variant={`surface`} color={`tomato`}>
                        <TextField.Slot>
                           <SearchIcon height="12" width="12" />
                        </TextField.Slot>
                        <TextField.Input placeholder="Search the docs…" />
                     </TextField.Root>
                  </Flex>
               </Tabs.Content>
            </Tabs.Root>
         </Flex>
      </Container>
   );
};

export default Page;
