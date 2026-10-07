"use client";

import * as PopoverPrimitive from "@radix-ui/react-popover";
import { cn } from "@/shared/utils/cn";

const Popover = PopoverPrimitive.Root;
const PopoverTrigger = PopoverPrimitive.Trigger;
const PopoverClose = PopoverPrimitive.Close;

function PopoverContent({ className, align = "start", sideOffset = 10, ...props }: React.ComponentProps<typeof PopoverPrimitive.Content>) {
  return <PopoverPrimitive.Portal><PopoverPrimitive.Content align={align} sideOffset={sideOffset} className={cn("z-50 w-72 rounded-xl border border-slate-200 bg-white p-4 text-slate-900 shadow-[0_18px_45px_rgba(0,12,29,.25)] outline-none data-[state=open]:animate-in data-[state=closed]:animate-out", className)} {...props} /></PopoverPrimitive.Portal>;
}

export { Popover, PopoverClose, PopoverContent, PopoverTrigger };
