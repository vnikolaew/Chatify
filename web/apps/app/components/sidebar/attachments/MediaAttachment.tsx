"use client";
import { ChatGroupAttachment } from "@openapi";
import { useCurrentUserId, useHover } from "@hooks";
import { Button, Card, CardHeader, Chip, Image, Tooltip } from "@nextui-org/react";
import { downloadImage } from "../../../utils";
import { DownloadIcon } from "lucide-react";
import { getMediaUrl } from "@web/api";
import moment from "moment/moment";
import React from "react";
import { useTranslations } from "next-intl";

export const MediaAttachment = ({
                                    attachment,
                                }: {
    attachment: ChatGroupAttachment;
}) => {
    const [cardRef, isHovered] = useHover<HTMLDivElement>();
    const meId = useCurrentUserId();
    const t = useTranslations(`Sidebar.SharedMedia`);

    return (
        <div
            className={`m-2 max-w-[150px] min-w-fit w-full flex flex-col gap-2`}
            key={attachment.attachmentId}
        >
            <Card radius={"md"} className={`w-full h-full`} ref={cardRef}>
                <CardHeader
                    className={`absolute flex-col items-end w-full p-0 z-20 top-1 right-1`}
                >
                    {isHovered && (
                        <Tooltip
                            closeDelay={100}
                            delay={100}
                            placement={"bottom"}
                            offset={-2}
                            showArrow
                            shadow={"md"}
                            radius={"sm"}
                            size={"sm"}
                            classNames={{
                                base: `px-3 py-0 text-[.6rem]`,
                            }}
                            content={"Download"}
                        >
                            <Button
                                size={"sm"}
                                onPress={(_) => {
                                    downloadImage(
                                        attachment.mediaInfo.mediaUrl,
                                        attachment.mediaInfo.fileName ?? "Untitled",
                                    );
                                }}
                                radius={"full"}
                                className={`hover:bg-zinc-900 gap-0 !h-6 !w-6 !px-0 transition-background duration-100 bg-zinc-900 bg-opacity-20 group`}
                                startContent={
                                    <DownloadIcon
                                        className={`fill-foreground stroke-foreground group-hover:fill-foreground group-hover:stroke-foreground`}
                                        size={12}
                                    />
                                }
                                isIconOnly
                            />
                        </Tooltip>
                    )}
                </CardHeader>
                <Image
                    alt={attachment.mediaInfo.fileName}
                    loading={`lazy`}
                    shadow={"lg"}
                    radius={"md"}
                    className={`relative w-full h-full object-cover`}
                    removeWrapper
                    src={getMediaUrl(attachment.mediaInfo.mediaUrl)}
                />
            </Card>
            <div className={`flex flex-col gap-0 items-start`}>
            <span className={`text-foreground text-medium`}>
               {attachment.mediaInfo.fileName ?? `Untitled`}
            </span>
                <p className={`text-default-500 leading-5 w-full text-[.7rem]`}>
                    {t.rich(`AttachmentDescription`,
                        {
                            name: attachment.userId === meId ?
                                ` ${attachment.username} (you)`
                                : attachment.username,
                            date: moment(new Date(attachment.createdAt)).format("DD MMM YYYY"),
                            nameBold: (chunks) => <b className={`text-foreground-700 text-xs`}>{chunks}</b>,
                            dateBold: (chunks) => <Chip className={`!text-[10px] h-[16px] ml-1 px-1 !py-0`} variant={`shadow`} color={`default`} size={`sm`} >{chunks}</Chip>,
                        })}
                </p>
            </div>
        </div>
    );
};
